using System;
using System.Linq;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.FileStorage;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Attachments.Delete;

public class DeleteAttachmentFilesCommandHandler 
    : IRequestHandler<DeleteAttachmentFilesCommand, Result>
{
    private readonly ILogger<DeleteAttachmentFilesCommandHandler> _logger;
    private readonly UnitOfWork _uow;
    private readonly IFileStorageService _fileStorageService;

    public DeleteAttachmentFilesCommandHandler(
        ILogger<DeleteAttachmentFilesCommandHandler> logger,
        UnitOfWork uow, 
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _uow = uow;
        _fileStorageService = fileStorageService;
    }

    public async System.Threading.Tasks.Task<Result> Handle(
        DeleteAttachmentFilesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (command.Attachments?.Any() == true)
            {
                foreach (var attachment in command.Attachments)
                {
                    await _fileStorageService.DeleteFile(attachment.FileId, cancellationToken);
                }
            }
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete attachment files. {@Command}", command);
            
            return Result.Internal("Failed to delete attachment files.");
        }
    }
}
