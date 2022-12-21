using FluentValidation;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class UpdateUserProfileAgregateDepartmentCommandValidator 
    : AbstractValidator<UpdateUserProfileAgregateDepartmentCommand>
{
    public UpdateUserProfileAgregateDepartmentCommandValidator()
    {
    }
}
