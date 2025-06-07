using Microsoft.AspNetCore.Mvc;
using UserDataApp.API.Models;
using UserDataApp.API.Services;

namespace UserDataApp.API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UserDataController : ControllerBase
  {
    private readonly IUserDataService _userDataService;

    public UserDataController(IUserDataService userDataService)
    {
      _userDataService = userDataService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserData>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? email = null,
        [FromQuery] string? comments = null,
        [FromQuery] string? title = null,
        [FromQuery] DateTime? registrationDateFrom = null,
        [FromQuery] DateTime? registrationDateTo = null,
        [FromQuery] string? gender = null,
        [FromQuery] string? country = null,
        [FromQuery] decimal? minSalary = null,
        [FromQuery] decimal? maxSalary = null,
        [FromQuery] DateTime? birthDateFrom = null,
        [FromQuery] DateTime? birthDateTo = null)
    {
      try
      {
        var result = await _userDataService.GetAllUsersAsync(
            page,
            pageSize,
            firstName,
            lastName,
            email,
            comments,
            title,
            registrationDateFrom,
            registrationDateTo,
            gender,
            country,
            minSalary,
            maxSalary,
            birthDateFrom,
            birthDateTo);

        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }
  }
}