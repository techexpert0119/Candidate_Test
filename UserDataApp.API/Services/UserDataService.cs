using Microsoft.Extensions.Caching.Memory;
using Parquet;
using Parquet.Data;
using System.Data;
using UserDataApp.API.Models;
using Parquet.Schema;

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
        var dataTable = await ReadParquetFileAsync();
        var allUsers = ConvertDataTableToUserData(dataTable);

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
      if (_cache.TryGetValue(CACHE_KEY, out DataTable? cachedData) && cachedData != null)
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
        if (field is DataField dataField)
        {
          dataTable.Columns.Add(field.Name, dataField.ClrType);
        }
        else
        {
          dataTable.Columns.Add(field.Name, typeof(string));
        }
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
            if (field is DataField dataField)
            {
              var dataColumn = await rowGroupReader.ReadColumnAsync(dataField);
              var value = dataColumn.Data.GetValue(j);
              row[field.Name] = value ?? DBNull.Value;
            }
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

    private List<UserData> ConvertDataTableToUserData(DataTable dataTable)
    {
      var users = new List<UserData>();
      foreach (DataRow row in dataTable.Rows)
      {
        var birthdateStr = Convert.ToString(row["birthdate"]);
        DateTime birthdate;
        if (string.IsNullOrEmpty(birthdateStr) || !DateTime.TryParse(birthdateStr, out birthdate))
        {
          birthdate = DateTime.Now; // Default to current date if parsing fails
        }

        decimal salary = 0;
        if (row["salary"] != DBNull.Value)
        {
          salary = Convert.ToDecimal(row["salary"]);
        }

        users.Add(new UserData
        {
          FirstName = Convert.ToString(row["first_name"]) ?? string.Empty,
          LastName = Convert.ToString(row["last_name"]) ?? string.Empty,
          Email = Convert.ToString(row["email"]) ?? string.Empty,
          Gender = Convert.ToString(row["gender"]) ?? string.Empty,
          Country = Convert.ToString(row["country"]) ?? string.Empty,
          Title = Convert.ToString(row["title"]) ?? string.Empty,
          Comments = Convert.ToString(row["comments"]) ?? string.Empty,
          RegistrationDate = DateTime.Now, // Since this isn't in the Parquet file, we'll use current date
          BirthDate = birthdate,
          Salary = salary
        });
      }
      return users;
    }
  }
}