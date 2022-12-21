using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Attachments.GetInfo;

public class GetAttachmentsInfoCommandHandler 
    : IRequestHandler<GetAttachmentsInfoCommand, Result<IEnumerable<TaskModel>>>
{
    private readonly ILogger<GetAttachmentsInfoCommandHandler> _logger;

    public GetAttachmentsInfoCommandHandler(
        ILogger<GetAttachmentsInfoCommandHandler> logger)
    {
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<Result<IEnumerable<TaskModel>>> Handle(
        GetAttachmentsInfoCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var result = command.TaskModels.Select(x =>
            {
                x.Attachments = x.Attachments.Select(y =>
                {
                    y.Url = "/legal/TaskFiles?AttachmentId=" + y.Id;
                    
                    return y;
                });
                
                return x;
            });
                

            return Result<IEnumerable<TaskModel>>.Ok(result);
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get task attachment urls.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<IEnumerable<TaskModel>>.Internal(failMsg);
        }
    }
}
