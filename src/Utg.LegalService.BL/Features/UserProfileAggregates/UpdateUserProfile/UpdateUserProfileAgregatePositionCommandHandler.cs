using System;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;

public class UpdateUserProfileAgregateCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public UpdateUserProfileAgregateCommandHandler(
        ILogger<UpdateUserProfileAgregateCommandHandler> logger, 
        IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateUserProfileAgregateCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var data = command.EventModel.Data;

            var existedAgregate = await _uow.UserProfileAgregatesRepository
                .GetQuery(x => x.UserProfileId == data.UserProfileId, null)
                .FirstOrDefaultAsync(cancellationToken);
            if (existedAgregate == null)
            {
                var entity = data.Adapt<UserProfileAgregate>();
                await _uow.UserProfileAgregatesRepository.AddAsync(entity, cancellationToken);
            }
            else
            {
                data.Adapt(existedAgregate);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to update UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
