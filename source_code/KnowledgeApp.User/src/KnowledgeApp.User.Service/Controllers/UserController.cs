using KnowledgeApp.Common;
using MassTransit;
using KnowledgeApp.User.Contracts;
using Microsoft.AspNetCore.Mvc;
using KnowledgeApp.User.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using KnowledgeApp.Authentication.JwtAuthenticationManager;
using KnowledgeApp.Authentication.JwtAuthenticationManager.Models;

namespace KnowledgeApp.User.Service.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<UserModel> _usersRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly JwtTokenHandler _jwtTokenHandler;

        public UsersController(IRepository<UserModel> usersRepository, IPublishEndpoint publishEndpoint, JwtTokenHandler jwtTokenHandler)
        {
            _usersRepository = usersRepository;
            _publishEndpoint = publishEndpoint;
            _jwtTokenHandler = jwtTokenHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var users = await _usersRepository.GetAllAsync();

                if (users == null || !users.Any())
                {
                    return NotFound(); // Return 404 if no Users found
                }

                var userDtos = users.Select(user => user.AsDto());
                return Ok(userDtos); // Return 200 with the data
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, "Internal server error: " + ex.Message); // Return 500 for unexpected errors
            }
        }
        // GET /Users/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await _usersRepository.GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user.AsDto();
        }

        // POST /Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostAsync(UserCreateDto userCreateDto)
        {
            var userModel = new UserModel
            {
                UserName = userCreateDto.UserName,
                Password = userCreateDto.Password,
                Role = userCreateDto.Role
            };

            await _usersRepository.CreateAsync(userModel);

            await _publishEndpoint.Publish(new UserCreated(userModel.Id, userModel.UserName));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = userModel.Id }, userModel);
        }

        // PUT /Users/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutAsync(Guid id, UserUpdateDto userUpdateDto)
        {
            var existingUser = await _usersRepository.GetAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.UserName = userUpdateDto.UserName;
            existingUser.Password = userUpdateDto.Password;
            existingUser.Role = userUpdateDto.Role;

            await _usersRepository.UpdateAsync(existingUser);

            await _publishEndpoint.Publish(new UserUpdated(existingUser.Id, existingUser.UserName));

            return NoContent();
        }

        // DELETE /Users/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var userModel = await _usersRepository.GetAsync(id);

            if (userModel == null)
            {
                return NotFound();
            }

            await _usersRepository.RemoveAsync(userModel.Id);

            await _publishEndpoint.Publish(new UserDeleted(id));

            return NoContent();
        }
    

        [HttpPost("auth")]
        public async Task<ActionResult<AuthenticationResponse?>> Authenticate([FromBody] AuthenticationRequest authenticationRequest)
        {
            try
            {
                Console.WriteLine("came to auth");
                var users = await _usersRepository.GetAllAsync();

                var userDtos = users.Select(user => user.AsDto()).ToList();
                var userFound = userDtos.FirstOrDefault(user => user.UserName == authenticationRequest.UserName);

                if (userFound == null)
                {
                    return Unauthorized(); // Return 401 if user is not found
                }

                // Generate JWT token for the found user
                var authenticationResponse = _jwtTokenHandler.GenerateJwtToken(authenticationRequest, userFound.UserName, userFound.Role);

                return Ok(authenticationResponse); // Return 200 OK with the JWT token
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, "Internal server error: " + ex.Message); // Return 500 for unexpected errors
            }

        }

    }
}