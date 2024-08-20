
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
            
            try {
                    await this._userRepository.AddUserAsync(new User {
                        UserId = Guid.NewGuid().ToString(),
                        Username = Username,
                        Password = Password
                    });

                    return Ok(); 
                }
            catch (InvalidOperationException error) {
                return new ObjectResult(error.Message) { StatusCode = StatusCodes.Status208AlreadyReported };
            } 
        }

        [HttpGet("/Alls")]
        public async Task<IActionResult> GetAllCustomers() 
        { 
            try {
                var users = await this._userRepository.GetAllUsersAsync();
                return Ok(users);
            } 
            catch (Exception error) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            } 
        } 
    }
}
