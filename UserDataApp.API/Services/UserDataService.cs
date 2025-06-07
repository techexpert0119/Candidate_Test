using Parquet;
using Parquet.Data;
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
    private readonly string _parquetFilePath;

    public UserDataService(IConfiguration configuration)
    {
      _parquetFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userdata.parquet");
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

    private async Task<IEnumerable<UserData>> ReadParquetFileAsync()
    {
      var users = new List<UserData>();

      if (!File.Exists(_parquetFilePath))
      {
        throw new FileNotFoundException($"Parquet file not found at: {_parquetFilePath}");
      }

      using (var stream = File.OpenRead(_parquetFilePath))
      {
        using var reader = await ParquetReader.CreateAsync(stream);
        var rowGroupCount = reader.RowGroupCount;

        for (int i = 0; i < rowGroupCount; i++)
        {
          using var rowGroupReader = reader.OpenRowGroupReader(i);
          var schema = reader.Schema;
          var dataFields = schema.GetDataFields();
          var columns = new Dictionary<string, Array>(StringComparer.OrdinalIgnoreCase);

          foreach (var field in dataFields)
          {
            var column = await rowGroupReader.ReadColumnAsync(field);
            columns[field.Name] = column.Data;
          }

          var rowCount = columns.First().Value.Length;

          for (int j = 0; j < rowCount; j++)
          {
            try
            {
              var firstName = columns["first_name"].GetValue(j)?.ToString() ?? string.Empty;
              var lastName = columns["last_name"].GetValue(j)?.ToString() ?? string.Empty;
              var email = columns["email"].GetValue(j)?.ToString() ?? string.Empty;
              var gender = columns["gender"].GetValue(j)?.ToString() ?? string.Empty;
              var country = columns["country"].GetValue(j)?.ToString() ?? string.Empty;
              var title = columns["title"].GetValue(j)?.ToString() ?? string.Empty;
              var comments = columns["comments"].GetValue(j)?.ToString() ?? string.Empty;

              var registrationDateStr = columns["registration_dttm"].GetValue(j)?.ToString();
              var birthDateStr = columns["birthdate"].GetValue(j)?.ToString();

              DateTime? registrationDate = null;
              DateTime? birthDate = null;

              if (!string.IsNullOrEmpty(registrationDateStr))
              {
                if (DateTime.TryParse(registrationDateStr, out DateTime parsedDate))
                {
                  registrationDate = parsedDate;
                }
              }

              if (!string.IsNullOrEmpty(birthDateStr))
              {
                if (DateTime.TryParse(birthDateStr, out DateTime parsedDate))
                {
                  birthDate = parsedDate;
                }
                else if (DateTime.TryParseExact(birthDateStr, "M/d/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime exactDate))
                {
                  birthDate = exactDate;
                }
              }

              var salary = Convert.ToDecimal(columns["salary"].GetValue(j));

              users.Add(new UserData
              {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Gender = gender,
                Country = country,
                Title = title,
                Comments = comments,
                RegistrationDate = registrationDate ?? DateTime.MinValue,
                BirthDate = birthDate ?? DateTime.MinValue,
                Salary = salary
              });
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error processing row {j}: {ex.Message}");
              throw;
            }
          }
        }
      }

      return users;
    }
  }
}