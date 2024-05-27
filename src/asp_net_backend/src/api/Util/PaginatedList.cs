namespace Restify.API.Util;

public class PaginatedList<T>
{
    private int PageNumber { get; set; }
    public int TotalPages { get; private set; }
    public List<T>  Values { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Values = items;
        PageNumber = pageNumber;
        TotalPages = (int) Math.Ceiling(count / (double) pageSize);
    }

    //public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
    //{
    //    var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
    //    return new PaginatedList<T>(items, items.Count, pageIndex, pageSize);
    //}
    
    public static PaginatedList<T> Create(IEnumerable<T> source, int count, int pageNumber, int pageSize)
    {
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
    
    //public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    //{
    //    var count = await source.CountAsync();
    //    var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
    //    return new PaginatedList<T>(items, count, pageIndex, pageSize);
    //}
}