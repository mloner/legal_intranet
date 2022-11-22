using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Utg.LegalService.Dal.Interceptors;

internal class DateTimeStampsInterceptor : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropsCache = new();
    private static readonly object Sync = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        => SavingChangesAsync(eventData, result).Result;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var baseResult = base.SavingChangesAsync(eventData, result, cancellationToken);
        var entries = GetEntries(eventData);

        if (entries == null)
            return await baseResult;

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();

            if (!PropsCache.TryGetValue(entityType, out var infos))
            {
                lock (Sync)
                {
                    if (!PropsCache.TryGetValue(entityType, out _))
                    {
                        infos = GetPropertyRules(entityType);
                        PropsCache.TryAdd(entityType, infos);
                    }
                }
            }

            if (infos == null)
                continue;

            foreach (var info in infos)
            {
                var value = info.GetValue(entry.Entity);
                if (value == null)
                    continue;

                var valueDateTime = (DateTime)value;
                if (valueDateTime.Kind == DateTimeKind.Unspecified)
                    continue;

                valueDateTime = valueDateTime.ToUniversalTime();
                valueDateTime = DateTime.SpecifyKind(valueDateTime, DateTimeKind.Unspecified);
                info.SetValue(entry.Entity, valueDateTime);
            }
        }

        return await baseResult;
    }

    private static List<PropertyInfo> GetPropertyRules(Type type)
    {
        var list = new List<PropertyInfo>();

        foreach (var info in type.GetProperties())
        {
            var valType = info.PropertyType;
            if (valType == typeof(DateTime) || valType == typeof(DateTime?))
                list.Add(info);
        }

        if (list.Count == 0)
            return null;

        list.TrimExcess();
        return list;
    }


    private List<EntityEntry> GetEntries(DbContextEventData eventData)
    {
        return eventData.Context?.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged
                        && e.State != EntityState.Detached)
            .ToList();
    }
}
