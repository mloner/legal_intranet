using FluentValidation;

namespace Utg.LegalService.BL.Features.Agregates.UpdateCompany;

/// <summary>
/// Валидаторы подключаются вместе с медиатором и позволяют
/// до начала выполнения провалидировать входящую команду
/// </summary>
public class UpdateUserProfileAgregateCompanyCommandValidator 
    : AbstractValidator<UpdateUserProfileAgregateCompanyCommand>
{
    public UpdateUserProfileAgregateCompanyCommandValidator()
    {
    }
}
