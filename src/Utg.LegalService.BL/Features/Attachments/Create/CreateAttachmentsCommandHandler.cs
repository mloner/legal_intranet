using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.FileStorage;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Attachments.Create;

public class CreateAttachmentsCommandHandler 
    : IRequestHandler<CreateAttachmentsCommand, Result<IEnumerable<TaskAttachmentModel>>>
{
    private readonly ILogger<CreateAttachmentsCommandHandler> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fileStorageService;

    public CreateAttachmentsCommandHandler(
        ILogger<CreateAttachmentsCommandHandler> logger,
        IUnitOfWork uow, 
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _uow = uow;
        _fileStorageService = fileStorageService;
    }

    public async System.Threading.Tasks.Task<Result<IEnumerable<TaskAttachmentModel>>> Handle(
        CreateAttachmentsCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (command.Attachments?.Any() != true)
            {
                return Result<IEnumerable<TaskAttachmentModel>>.Ok(
                    new List<TaskAttachmentModel>());
            }
            
            var customAttachments = new List<TaskAttachment>();

            foreach (var attachment in command.Attachments)
            {
                var attachmentFileId =
                    await _fileStorageService.SaveFile(attachment.OpenReadStream(),
                        attachment.ContentType,
                        cancellationToken);

                customAttachments.Add(
                    new TaskAttachment()
                    {
                        TaskId = command.TaskId,
                        FileId = attachmentFileId,
                        FileName = attachment.FileName,
                        FileSizeInBytes = attachment.Length,
                        UserProfileId = command.AuthInfo.UserProfileId
                    });
            }

            await _uow.TaskAttachmentRepository
                .AddRange(customAttachments, cancellationToken);
            
            return Result<IEnumerable<TaskAttachmentModel>>.Ok(
                customAttachments.Adapt<IEnumerable<TaskAttachmentModel>>());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add subtask. {@Command}", command);
            
            return Result<IEnumerable<TaskAttachmentModel>>.Internal("Failed to add subtask.");
        }
    }
}
