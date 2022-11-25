using FluentValidation;

namespace Utg.LegalService.BL.Features.Agregates.Fill;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class FillUserProfileAgregatesCommandValidator 
    : AbstractValidator<FillUserProfileAgregatesCommand>
{
    public FillUserProfileAgregatesCommandValidator()
    {
    }
}
