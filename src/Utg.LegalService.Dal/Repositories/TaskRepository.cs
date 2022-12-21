using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Utg.Common.EF.Repositories.Implementations;
using Utg.Common.Packages.Domain.Exceptions;
using Utg.LegalService.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;
using Task = Utg.LegalService.Common.Models.Domain.Task;

namespace Utg.LegalService.Dal.Repositories
{
    public class TaskRepository 
        : BaseRepositoryAdvanced<UtgContext, Task>, ITaskRepository
    {
        private readonly UtgContext _context;
        private readonly IMapper _mapper;

        public TaskRepository(
            UtgContext context,
            IMapper mapper)
        : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public IQueryable<Task> Get()
        {
            return _context.Tasks
                .AsQueryable();
            
        }

        private async Task<Task> GetTaskEntity(int id)
        {
            var entity = await _context.Tasks
                .Include(x => x.TaskAttachments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }

            return entity;
        }

        public async Task<TaskModel> GetById(int id)
        {
            var entity = await GetTaskEntity(id);
            var model = _mapper.Map<TaskModel>(entity);

            return model;
        }

        public async Task<TaskModel> CreateTask(TaskModel inputModel)
        {
            var entity = _mapper.Map<Task>(inputModel);
            _context.Tasks.Add(entity);
            await _context.SaveChangesAsync();
            var model = _mapper.Map<TaskModel>(entity);
            return model;
        }

        public async System.Threading.Tasks.Task UpdateTask(TaskModel model)
        {
            var entity = await GetTaskEntity(model.Id);
            _mapper.Map(model, entity);
            _context.Entry(entity).Property(x => x.Status).IsModified = true;
            _context.Entry(entity).Property(x => x.Type).IsModified = true;
            _context.Entry(entity).Property(x => x.Description).IsModified = true;
            _context.Entry(entity).Property(x => x.AuthorUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.CreationDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.PerformerUserProfileId).IsModified = true;
            _context.Entry(entity).Property(x => x.DeadlineDateTime).IsModified = true;
            _context.Entry(entity).Property(x => x.LastChangeDateTime).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public System.Threading.Tasks.Task UpdateTaskRange(IEnumerable<Task> entites,
            CancellationToken cancellationToken = default)
        {
            _context.UpdateRange(entites);
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async System.Threading.Tasks.Task DeleteTask(int taskId)
        {
            var entity = await GetTaskEntity(taskId);
            _context.Tasks.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskMoveToInWork(TaskModel model)
        {
            var entity = await GetTaskEntity(model.Id);
            _mapper.Map(model, entity);
            _context.Entry(entity).Property(x => x.Status).IsModified = true;
            _context.Entry(entity).Property(x => x.Type).IsModified = false;
            _context.Entry(entity).Property(x => x.Description).IsModified = false;
            _context.Entry(entity).Property(x => x.AuthorUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.CreationDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.PerformerUserProfileId).IsModified = true;
            _context.Entry(entity).Property(x => x.DeadlineDateTime).IsModified = true;
            _context.Entry(entity).Property(x => x.LastChangeDateTime).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskMoveToUnderReview(TaskModel model)
        {
            var entity = await GetTaskEntity(model.Id);
            _mapper.Map(model, entity);
            _context.Entry(entity).Property(x => x.Status).IsModified = true;
            _context.Entry(entity).Property(x => x.Type).IsModified = false;
            _context.Entry(entity).Property(x => x.Description).IsModified = false;
            _context.Entry(entity).Property(x => x.AuthorUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.CreationDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.PerformerUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.DeadlineDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.LastChangeDateTime).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskMoveToDone(TaskModel model)
        {
            var entity = await GetTaskEntity(model.Id);
            _mapper.Map(model, entity);
            _context.Entry(entity).Property(x => x.Status).IsModified = true;
            _context.Entry(entity).Property(x => x.Type).IsModified = false;
            _context.Entry(entity).Property(x => x.Description).IsModified = false;
            _context.Entry(entity).Property(x => x.AuthorUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.CreationDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.PerformerUserProfileId).IsModified = false;
            _context.Entry(entity).Property(x => x.DeadlineDateTime).IsModified = false;
            _context.Entry(entity).Property(x => x.LastChangeDateTime).IsModified = true;

            await _context.SaveChangesAsync();
        }
    }
}
