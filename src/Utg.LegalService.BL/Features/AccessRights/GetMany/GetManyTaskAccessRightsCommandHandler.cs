using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.AccessRights.GetMany;

public class GetManyTaskAccessRightsCommandHandler 
    : IRequestHandler<GetManyTaskAccessRightsCommand, Result<IEnumerable<TaskAccessRights>>>
{
    private readonly ILogger<GetManyTaskAccessRightsCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;

    public GetManyTaskAccessRightsCommandHandler(
        ILogger<GetManyTaskAccessRightsCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<Result<IEnumerable<TaskAccessRights>>> Handle(
        GetManyTaskAccessRightsCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var tars = new List<TaskAccessRights>();
            foreach (var commandTask in command.Tasks)
            {
                var getTarCommand = new GetTaskAccessRightsCommand()
                {
                    Task = commandTask,
                    AuthInfo = command.AuthInfo
                };
                var getTarsCommandResp = 
                    await _mediator.Send(getTarCommand, cancellationToken);
                if(!getTarsCommandResp.Success)
                    return Result<IEnumerable<TaskAccessRights>>.Failed(getTarsCommandResp);
                tars.Add(getTarsCommandResp.Data);
            }

            return Result<IEnumerable<TaskAccessRights>>.Ok(tars);
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get many task access rights.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<IEnumerable<TaskAccessRights>>.Internal(failMsg);
        }
    }
}
