using KnowledgeApp.Common;
using MassTransit;
using KnowledgeApp.User.Contracts;
using Microsoft.AspNetCore.Mvc;
using KnowledgeApp.User.Service.Models;

namespace KnowledgeApp.User.Service.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<UserModel> _usersRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UsersController(IRepository<UserModel> usersRepository, IPublishEndpoint publishEndpoint)
        {
            _usersRepository = usersRepository;
            _publishEndpoint = publishEndpoint;
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
                Password = userCreateDto.Password
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
    }
}