using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Common.Models;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;

public class UpdateUserProfileAgregateCompanyCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateCompanyCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateCompanyCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public UpdateUserProfileAgregateCompanyCommandHandler(
        ILogger<UpdateUserProfileAgregateCompanyCommandHandler> logger, 
        IUnitOfWork uow)
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
                var upas = _uow.UserProfileAgregatesRepository
                    .GetQuery(x => x.CompanyId == data.Id, null);
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
            var failMsg = "Failed to update UserProfileAgregate companies.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
