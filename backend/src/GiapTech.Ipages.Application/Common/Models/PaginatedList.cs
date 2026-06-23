namespace GiapTech.Ipages.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginatedList<T> Create(IEnumerable<T> source, int totalCount, int page, int pageSize)
        => new() { Items = source.ToList(), TotalCount = totalCount, Page = page, PageSize = pageSize };
}
