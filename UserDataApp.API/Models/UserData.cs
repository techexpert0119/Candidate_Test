using System;

namespace UserDataApp.API.Models
{
  public class UserData
  {
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime BirthDate { get; set; }
    public decimal Salary { get; set; }
  }
}