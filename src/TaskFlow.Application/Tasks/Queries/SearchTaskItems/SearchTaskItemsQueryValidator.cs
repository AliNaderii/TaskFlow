using FluentValidation;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Queries.SearchTaskItems;

internal sealed class SearchTaskItemsQueryValidator
    : AbstractValidator<SearchTaskItemsQuery>
{
    private static readonly string[] AllowedSortBy =
    {
        "title", "status", "priority", "duedate", "createdat", "updatedat"
    };

    private static readonly string[] AllowedSortDirection = { "asc", "desc" };

    public SearchTaskItemsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be greater than or equal to 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be less than or equal to 100.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortBy)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}.");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection))
            .WithMessage($"SortDirection must be one of: {string.Join(", ", AllowedSortDirection)}.");

        RuleFor(x => x.DueDateTo)
            .GreaterThanOrEqualTo(x => x.DueDateFrom)
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue)
            .WithMessage("DueDateTo must be greater than or equal to DueDateFrom.");
    }

    private static bool BeValidSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) || 
               AllowedSortBy.Contains(sortBy.ToLowerInvariant());
    }

    private static bool BeValidSortDirection(string? sortDirection)
    {
        return string.IsNullOrWhiteSpace(sortDirection) || 
               AllowedSortDirection.Contains(sortDirection.ToLowerInvariant());
    }
}