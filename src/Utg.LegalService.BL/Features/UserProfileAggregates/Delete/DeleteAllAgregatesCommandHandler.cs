using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.Delete;

public class DeleteAllAgregatesCommandHandler : IRequestHandler<DeleteAllAgregatesCommand, Result>
{
    private readonly ILogger<DeleteAllAgregatesCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public DeleteAllAgregatesCommandHandler(
        ILogger<DeleteAllAgregatesCommandHandler> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteAllAgregatesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            await _uow.UserProfileAgregatesRepository.RemoveAllAsync(cancellationToken);
             
            _logger.LogInformation("All UserProfileAgregates removed"); 
            return Result.Ok();
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to remove UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
