using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.Delete;

public class DeleteAllAgregatesCommandHandler : IRequestHandler<DeleteAllAgregatesCommand, Result>
{
    private readonly ILogger<DeleteAllAgregatesCommandHandler> _logger;
    private readonly UnitOfWork _uow;

    public DeleteAllAgregatesCommandHandler(
        ILogger<DeleteAllAgregatesCommandHandler> logger, UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteAllAgregatesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var items = _uow.AgregateItems.GetQuery(x => true, null);
            _uow.AgregateItems.RemoveRange(items);
            await _uow.SaveChangesAsync(cancellationToken);
             
            _logger.LogInformation("UserProfileAgregates removed {@Items}", items); 
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove UserProfileAgregates. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result.Internal("Failed to remove UserProfileAgregates.");
        }
    }
}
