using FluentValidation;

namespace Utg.LegalService.BL.Features.Agregates.Delete;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class DeleteAllAgregatesCommandValidator : AbstractValidator<DeleteAllAgregatesCommand>
{
    public DeleteAllAgregatesCommandValidator()
    {
    }
}
