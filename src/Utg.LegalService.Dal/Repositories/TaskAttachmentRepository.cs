using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Dal.Repositories
{
    public class TaskAttachmentRepository 
        : BaseRepositoryAdvanced<TaskAttachment>, ITaskAttachmentRepository
    {
        private readonly UtgContext _context;
        private readonly IMapper _mapper;

        public TaskAttachmentRepository(
            UtgContext context,
            IMapper mapper)
            : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public IQueryable<TaskAttachment> Get()
        {
            return _context.TaskAttachments.AsQueryable();
        }

        public async Task<IEnumerable<TaskAttachmentModel>> CreateAttachments(int taskId, IEnumerable<TaskAttachmentModel> attachments)
        {
            var entities = _mapper.Map<IEnumerable<TaskAttachment>>(attachments);
            await _context.TaskAttachments.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            
            var models = _mapper.Map<IEnumerable<TaskAttachmentModel>>(entities);
            return models;
        }

        public async System.Threading.Tasks.Task RemoveAttachments(int taskId, IEnumerable<int> attachmentIds)
        {
            var attachmentsToRemove = await _context.TaskAttachments
                .Where(attachment => attachment.TaskId == taskId &&
                                     attachmentIds.Contains(attachment.Id))
                .ToArrayAsync();

            _context.TaskAttachments.RemoveRange(attachmentsToRemove);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TaskAttachment taskAttachment)
        {
            _context.TaskAttachments.Remove(taskAttachment);
            await _context.SaveChangesAsync();
        }
    }
}