namespace UserDataApp.API.Models;

public class PaginatedResponse<T>
{
  public IEnumerable<T> Data { get; set; } = new List<T>();
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }
  public int PageSize { get; set; }
}