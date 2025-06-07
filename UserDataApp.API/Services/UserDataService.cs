using Microsoft.Extensions.Caching.Memory;
using Parquet;
using Parquet.Data;
using System.Data;
using UserDataApp.API.Models;

namespace UserDataApp.API.Services
{
  public interface IUserDataService
  {
    Task<PaginatedResponse<UserData>> GetAllUsersAsync(
        int page = 1,
        int pageSize = 10,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? comments = null,
        string? title = null,
        DateTime? registrationDateFrom = null,
        DateTime? registrationDateTo = null,
        string? gender = null,
        string? country = null,
        decimal? minSalary = null,
        decimal? maxSalary = null,
        DateTime? birthDateFrom = null,
        DateTime? birthDateTo = null);
  }

  public class UserDataService : IUserDataService
  {
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "UserDataTable";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

    public UserDataService(IConfiguration configuration, IMemoryCache cache)
    {
      _configuration = configuration;
      _cache = cache;
    }

    public async Task<PaginatedResponse<UserData>> GetAllUsersAsync(
        int page = 1,
        int pageSize = 10,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? comments = null,
        string? title = null,
        DateTime? registrationDateFrom = null,
        DateTime? registrationDateTo = null,
        string? gender = null,
        string? country = null,
        decimal? minSalary = null,
        decimal? maxSalary = null,
        DateTime? birthDateFrom = null,
        DateTime? birthDateTo = null)
    {
      try
      {
        var allUsers = await ReadParquetFileAsync();

        // Apply filters
        var filteredUsers = allUsers.Where(user =>
            (string.IsNullOrEmpty(firstName) || user.FirstName.Contains(firstName, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(lastName) || user.LastName.Contains(lastName, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(email) || user.Email.Contains(email, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(comments) || user.Comments.Contains(comments, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(title) || user.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
            (!registrationDateFrom.HasValue || user.RegistrationDate >= registrationDateFrom.Value) &&
            (!registrationDateTo.HasValue || user.RegistrationDate <= registrationDateTo.Value) &&
            (string.IsNullOrEmpty(gender) || user.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(country) || user.Country.Equals(country, StringComparison.OrdinalIgnoreCase)) &&
            (!minSalary.HasValue || user.Salary >= minSalary.Value) &&
            (!maxSalary.HasValue || user.Salary <= maxSalary.Value) &&
            (!birthDateFrom.HasValue || user.BirthDate >= birthDateFrom.Value) &&
            (!birthDateTo.HasValue || user.BirthDate <= birthDateTo.Value))
            .ToList();

        var totalCount = filteredUsers.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var skip = (page - 1) * pageSize;

        var paginatedUsers = filteredUsers
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<UserData>
        {
          Data = paginatedUsers,
          TotalCount = totalCount,
          TotalPages = totalPages,
          CurrentPage = page,
          PageSize = pageSize
        };
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error in GetAllUsersAsync: {ex.Message}");
        throw;
      }
    }

    private async Task<DataTable> ReadParquetFileAsync()
    {
      if (_cache.TryGetValue(CACHE_KEY, out DataTable cachedData))
      {
        return cachedData;
      }

      var filePath = _configuration["ParquetFilePath"];
      if (string.IsNullOrEmpty(filePath))
      {
        throw new InvalidOperationException("Parquet file path is not configured");
      }

      using var stream = File.OpenRead(filePath);
      using var reader = await ParquetReader.CreateAsync(stream);
      var dataTable = new DataTable();

      // Read schema and create columns
      var schema = reader.Schema;
      foreach (var field in schema.Fields)
      {
        dataTable.Columns.Add(field.Name, GetClrType(field.DataType));
      }

      // Read data
      var rowGroupCount = reader.RowGroupCount;
      for (int i = 0; i < rowGroupCount; i++)
      {
        using var rowGroupReader = reader.OpenRowGroupReader(i);
        var rowCount = rowGroupReader.RowCount;
        var dataFields = schema.Fields.ToArray();

        for (int j = 0; j < rowCount; j++)
        {
          var row = dataTable.NewRow();
          for (int k = 0; k < dataFields.Length; k++)
          {
            var field = dataFields[k];
            var dataColumn = await rowGroupReader.ReadColumnAsync(field);
            row[field.Name] = dataColumn.Data.GetValue(j);
          }
          dataTable.Rows.Add(row);
        }
      }

      // Cache the data
      var cacheOptions = new MemoryCacheEntryOptions()
          .SetAbsoluteExpiration(CACHE_DURATION)
          .SetSlidingExpiration(TimeSpan.FromMinutes(10));

      _cache.Set(CACHE_KEY, dataTable, cacheOptions);

      return dataTable;
    }

    private Type GetClrType(Parquet.Data.DataType dataType)
    {
      switch (dataType)
      {
        case Parquet.Data.DataType.Int32:
          return typeof(int);
        case Parquet.Data.DataType.Int64:
          return typeof(long);
        case Parquet.Data.DataType.Float:
          return typeof(float);
        case Parquet.Data.DataType.Double:
          return typeof(double);
        case Parquet.Data.DataType.Boolean:
          return typeof(bool);
        case Parquet.Data.DataType.String:
          return typeof(string);
        case Parquet.Data.DataType.Date:
          return typeof(DateTime);
        case Parquet.Data.DataType.Time:
          return typeof(TimeSpan);
        case Parquet.Data.DataType.Timestamp:
          return typeof(DateTime);
        case Parquet.Data.DataType.Decimal:
          return typeof(decimal);
        default:
          throw new NotSupportedException($"Data type {dataType} is not supported");
      }
    }
  }
}