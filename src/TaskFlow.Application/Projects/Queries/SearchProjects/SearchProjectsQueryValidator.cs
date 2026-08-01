using FluentValidation;

namespace TaskFlow.Application.Projects.Queries.SearchProjects;

public sealed class SearchProjectsQueryValidator
    : AbstractValidator<SearchProjectsQuery>
{
    public SearchProjectsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || 
                new[] { "name", "createdat", "updatedat", "archivedat" }
                    .Contains(x.ToLowerInvariant()))
            .WithMessage("Invalid sort field.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || 
                new[] { "asc", "desc" }.Contains(x.ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}