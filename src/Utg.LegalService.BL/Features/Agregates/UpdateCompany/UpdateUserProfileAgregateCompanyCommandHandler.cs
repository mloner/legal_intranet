using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.UpdateCompany;

public class UpdateUserProfileAgregateCompanyCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateCompanyCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateCompanyCommandHandler> _logger;
    private readonly UnitOfWork _uow;

    public UpdateUserProfileAgregateCompanyCommandHandler(
        ILogger<UpdateUserProfileAgregateCompanyCommandHandler> logger, 
        UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateUserProfileAgregateCompanyCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var data = command.EventModel.Data;
            if (command.EventModel.Type == UpdateEventType.Update)
            {
                var upas = _uow.AgregateItems.GetQuery(x => x.CompanyId == data.Id, null);
                foreach (var upa in upas)
                {
                    upa.CompanyName = data.Name;
                }
                await _uow.SaveChangesAsync(cancellationToken);
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update UserProfileAgregate companies. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result.Internal("Failed to update UserProfileAgregate companies.");
        }
    }
}
