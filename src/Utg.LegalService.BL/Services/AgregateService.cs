using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.BL.Services;

public class AgregateService : IAgregateService
{
    private readonly IAgregateRepository _agregateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregateService> _logger;
    private readonly IDataProxyClient _dataProxyClient;

    public AgregateService(
        IAgregateRepository agregateRepository,
        IMapper mapper,
        ILogger<AgregateService> logger,
        IDataProxyClient dataProxyClient)
    {
        _agregateRepository = agregateRepository;
        _mapper = mapper;
        _logger = logger;
        _dataProxyClient = dataProxyClient;
    }

    public async Task FillUserProfiles()
    {
        var allUserProfiles = (await _dataProxyClient.AddressBookAsync()).Result;
        var allUserProfileIds = allUserProfiles.Select(x => x.Id);
        var existedUserProfileIds = await _agregateRepository.GetUserProfiles()
            .Select(x => x.UserProfileId)
            .ToListAsync();
        var needToAddUserProfileIds = allUserProfileIds.Except(existedUserProfileIds);
        if (needToAddUserProfileIds.Any())
        {
            var needToAddUserProfiles = allUserProfiles.Where(x => needToAddUserProfileIds.Contains(x.Id));
            var needToAddUserProfileEntities =
                _mapper.Map<IEnumerable<UserProfileAgregate>>(needToAddUserProfiles);
            await _agregateRepository.AddUserProfiles(needToAddUserProfileEntities);
        }
    }

    public async Task<IEnumerable<UserProfileAgregateModel>> GetUserProfiles(IEnumerable<int> ids)
    {
        var userProfileAgregates = await _agregateRepository
            .GetUserProfiles()
            .AsNoTracking()
            .Where(upa => ids.Contains(upa.UserProfileId))
            .ProjectTo<UserProfileAgregateModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        var existingUserProfileIds = userProfileAgregates.Select(upa => upa.UserProfileId);
        var notExistingUserProfileIds = ids.Except(existingUserProfileIds);
        if (notExistingUserProfileIds.Any())
        {
            _logger.LogError("UserProfile doesn't exist in user profiles agregate table {userProfileIds}",
                JsonConvert.SerializeObject(notExistingUserProfileIds, new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
            throw new Exception();
        }

        return userProfileAgregates;
    }
}