using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.MessageBus;
using Web.Services.AuthAPI.Models.Dto;
using Web.Services.AuthAPI.RabbitMQSender;
using Web.Services.AuthAPI.Service.IService;

namespace Web.Services.AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRabbitMQAuthMessageSender _messageBus;
        private readonly IConfiguration _configuration;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService, IRabbitMQAuthMessageSender messageBus, IConfiguration configuration)
        {
            _authService = authService;
            _messageBus = messageBus;
            _response = new();
            _configuration = configuration;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            _response.Message = "Registered successfully !!!";
            _messageBus.SendMessage(model.Email, _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            _response.Message = "Login successfully !!!";
            return Ok(_response);
        }

        [HttpPost("assignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            _response.Message = "Assign Role successfully !!!";
            return Ok(_response);
        }
    }
}
