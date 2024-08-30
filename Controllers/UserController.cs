
using Microsoft.AspNetCore.Mvc;
using ProjectModels;

namespace ProjectControllers {


    [Route("/[controller]")]
    [ApiController]
    public class UserController: ControllerBase {

        private readonly IUserRepository _userRepository;
        
        public UserController(IUserRepository userRepository) {
            this._userRepository = userRepository;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateUser(
            string Username,
            string? Password
        ) 
        {
            await this._userRepository.AddUserAsync(new User {
                UserId = Guid.NewGuid().ToString(),
                Username = Username,
                Password = Password
            });

            return Ok(); 
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllCustomers() 
        { 
            var users = await this._userRepository.GetAllUsersAsync();
            return Ok(users);
        } 
    }
}
