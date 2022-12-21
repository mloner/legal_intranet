using FluentValidation;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class UpdateUserProfileAgregateCommandValidator 
    : AbstractValidator<UpdateUserProfileAgregateCommand>
{
    public UpdateUserProfileAgregateCommandValidator()
    {
    }
}
