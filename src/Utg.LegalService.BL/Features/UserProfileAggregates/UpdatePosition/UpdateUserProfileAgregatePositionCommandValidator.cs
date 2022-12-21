using FluentValidation;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class UpdateUserProfileAgregatePositionCommandValidator 
    : AbstractValidator<UpdateUserProfileAgregatePositionCommand>
{
    public UpdateUserProfileAgregatePositionCommandValidator()
    {
    }
}
