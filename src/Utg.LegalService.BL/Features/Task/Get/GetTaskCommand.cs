using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.Get;

public class GetTaskCommand :  IRequest<Result<TaskModel>>
{
    public int Id { get; set; }
}
