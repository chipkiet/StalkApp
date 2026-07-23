using System.Linq;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IGenericRepository<User> _userRepository;

    public UsersController(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Demo accounts for the MVP login screen.</summary>
    [HttpGet("demo")]
    public async Task<IActionResult> GetDemoUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var result = users
            .OrderBy(u => u.DisplayName)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.DisplayName,
                u.AvatarUrl,
                u.IsOnline
            });
        return Ok(result);
    }
}
