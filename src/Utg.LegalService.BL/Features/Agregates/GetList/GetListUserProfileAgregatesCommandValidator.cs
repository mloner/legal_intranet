using FluentValidation;

namespace Utg.LegalService.BL.Features.Agregates.GetList;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class GetListUserProfileAgregatesCommandValidator 
    : AbstractValidator<GetListUserProfileAgregatesCommand>
{
    public GetListUserProfileAgregatesCommandValidator()
    {
    }
}
