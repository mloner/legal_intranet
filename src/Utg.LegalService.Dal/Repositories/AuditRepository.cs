using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly UtgContext _context;
        private readonly IMapper _mapper;

        public TaskRepository(
            UtgContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IQueryable<TaskModel> Get()
        {
            return _context.Tasks
                .AsQueryable()
                .ProjectTo<TaskModel>(_mapper.ConfigurationProvider);
        }
    }
}
