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
        private readonly IRequestClient<UserCreated> _userCreatedClient;
        private readonly IRequestClient<UserUpdated> _userUpdatedClient;
        private readonly IRequestClient<UserDeleted> _userDeletedClient;
        private readonly JwtTokenHandler _jwtTokenHandler;

        public UsersController(IRepository<UserModel> usersRepository, IPublishEndpoint publishEndpoint, JwtTokenHandler jwtTokenHandler,
            IRequestClient<UserCreated> userCreatedClient, IRequestClient<UserUpdated> userUpdatedClient, IRequestClient<UserDeleted> userDeletedClient)
        {
            _usersRepository = usersRepository;
            _publishEndpoint = publishEndpoint;
            _jwtTokenHandler = jwtTokenHandler;
            _userCreatedClient = userCreatedClient;
            _userUpdatedClient = userUpdatedClient;
            _userDeletedClient = userDeletedClient;
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
                Id = Guid.NewGuid(),
                UserName = userCreateDto.UserName,
                Password = userCreateDto.Password,
                Role = userCreateDto.Role
            };

            // Create the user in the User service
            await _usersRepository.CreateAsync(userModel);

            try
            {
                // Send the UserCreated request and await a response
                var response = await _userCreatedClient.GetResponse<UserCreatedResponse>(new UserCreated(userModel.Id, userModel.UserName));

                if (!response.Message.IsSuccessful)
                {
                    // Roll back the user creation if the response indicates failure
                    await _usersRepository.RemoveAsync(userModel.Id);
                    return StatusCode(StatusCodes.Status500InternalServerError, $"User creation failed: {response.Message.Message}");
                }
            }
            catch (Exception ex)
            {
                // Handle any communication errors
                Console.WriteLine($"Error during user creation: {ex.Message}");
                await _usersRepository.RemoveAsync(userModel.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, "User creation failed due to an unexpected error.");
            }

            // Return the created user
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

            var oldUserName = existingUser.UserName;
            var oldPassword = existingUser.Password;
            var oldRole = existingUser.Role;

            // Update local data in anticipation of success
            existingUser.UserName = userUpdateDto.UserName;
            existingUser.Password = userUpdateDto.Password;
            existingUser.Role = userUpdateDto.Role;

            await _usersRepository.UpdateAsync(existingUser);

            try
            {
                // Send update request and await response
                var response = await _userUpdatedClient.GetResponse<UserUpdatedResponse>(new UserUpdated(existingUser.Id, existingUser.UserName));

                if (!response.Message.IsSuccessful)
                {
                    // Rollback if the update was unsuccessful
                    existingUser.UserName = oldUserName;
                    existingUser.Password = oldPassword;
                    existingUser.Role = oldRole;

                    await _usersRepository.UpdateAsync(existingUser);
                    return StatusCode(StatusCodes.Status500InternalServerError, $"User update failed: {response.Message.Message}");
                }
            }
            catch (Exception ex)
            {
                // Handle communication error by rolling back
                existingUser.UserName = oldUserName;
                existingUser.Password = oldPassword;
                existingUser.Role = oldRole;

                await _usersRepository.UpdateAsync(existingUser);
                Console.WriteLine($"Error during user update: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "User update failed due to an unexpected error.");
            }

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

            // Delete locally and publish deletion request
            await _usersRepository.RemoveAsync(userModel.Id);

            try
            {
                // Send delete request and await response
                var response = await _userDeletedClient.GetResponse<UserDeletedResponse>(new UserDeleted(userModel.Id));

                if (!response.Message.IsSuccessful)
                {
                    // Rollback the delete if the response indicates failure
                    await _usersRepository.CreateAsync(userModel);
                    return StatusCode(StatusCodes.Status500InternalServerError, $"User deletion failed: {response.Message.Message}");
                }
            }
            catch (Exception ex)
            {
                // Rollback in case of communication error
                await _usersRepository.CreateAsync(userModel);
                Console.WriteLine($"Error during user deletion: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "User deletion failed due to an unexpected error.");
            }

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