using FluentValidation;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;

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
