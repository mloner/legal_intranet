using System;
using System.Linq;
using FluentValidation;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.GetOrAddRange;

public class GetOrAddRangeProfileAggregateCommandValidator : AbstractValidator<GetOrAddRangeProfileAggregateCommand>
{
    public GetOrAddRangeProfileAggregateCommandValidator()
    {
        RuleFor(q => q.TabNs)
            .NotEmpty()
            .When(q =>
                q.UserIds?.Any() != true
                && q.UserProfileIds?.Any() != true
                && q.OuterIds?.Any() != true
                && q.PersonIds?.Any() != true)
            .WithMessage($"Invalid filter. Only one and at least one list must by not empty. " +
                         $"'{nameof(GetOrAddRangeProfileAggregateCommand.TabNs)}' validation{Environment.NewLine}");

        RuleFor(q => q.UserIds)
            .NotEmpty()
            .When(q =>
                q.TabNs?.Any() != true
                && q.UserProfileIds?.Any() != true
                && q.OuterIds?.Any() != true
                && q.PersonIds?.Any() != true)
            .WithMessage($"Invalid filter. Only one and at least one list must by not empty. " +
                         $"'{nameof(GetOrAddRangeProfileAggregateCommand.UserIds)}' validation{Environment.NewLine}");

        RuleFor(q => q.UserProfileIds)
            .NotEmpty()
            .When(q =>
                q.TabNs?.Any() != true
                && q.UserIds?.Any() != true
                && q.OuterIds?.Any() != true
                && q.PersonIds?.Any() != true)
            .WithMessage($"Invalid filter. Only one and at least one list must by not empty. " +
                         $"'{nameof(GetOrAddRangeProfileAggregateCommand.UserProfileIds)}' validation{Environment.NewLine}");

        RuleFor(q => q.PersonIds)
            .NotEmpty()
            .When(q =>
                q.TabNs?.Any() != true
                && q.UserIds?.Any() != true
                && q.OuterIds?.Any() != true
                && q.UserProfileIds?.Any() != true)
            .WithMessage($"Invalid filter. Only one and at least one list must by not empty. " +
                         $"'{nameof(GetOrAddRangeProfileAggregateCommand.PersonIds)}' validation{Environment.NewLine}");

        RuleFor(q => q.OuterIds)
            .NotEmpty()
            .When(q =>
                q.TabNs?.Any() != true
                && q.UserIds?.Any() != true
                && q.UserProfileIds?.Any() != true
                && q.PersonIds?.Any() != true)
            .WithMessage($"Invalid filter. Only one and at least one list must by not empty. " +
                         $"'{nameof(GetOrAddRangeProfileAggregateCommand.OuterIds)}' validation{Environment.NewLine}");
    }
}
