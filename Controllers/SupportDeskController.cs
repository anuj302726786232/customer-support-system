using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportDeskAPI.Dto;
using SupportDeskAPI.Services;

namespace SupportDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportDeskController : ControllerBase
    {
        private readonly SupportDeskService _supportDeskService;

        public SupportDeskController(SupportDeskService supportDeskService)
        {
            _supportDeskService = supportDeskService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUser registerUser)
        {
            try
            {
                if(!ModelState.IsValid) 
                    return BadRequest(new { success = false, result = "Invalid credentials" });    
                
                var response = await _supportDeskService.CretaeUserAsync(registerUser);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "ValidationError" => BadRequest(new { success = false, result = response.ErrorMessage }),
                        "AlreadyUserExist" => BadRequest(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, (new { success = false, result = response.ErrorMessage }))
                    };

                return StatusCode(StatusCodes.Status201Created, (new { success = response.Success, result = $"{registerUser.UserName} created." }));
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }
    }
}
