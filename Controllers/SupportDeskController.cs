using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportDeskAPI.Dto;
using SupportDeskAPI.Entity;
using SupportDeskAPI.Services;
using System.Security.Claims;

namespace SupportDeskAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SupportDeskController : ControllerBase
    {
        private readonly SupportDeskService _supportDeskService;

        public SupportDeskController(SupportDeskService supportDeskService)
        {
            _supportDeskService = supportDeskService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUser registerUser)
        {
            try
            {
                if (!ModelState.IsValid)
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

        [HttpPost("createticket")]
        public async Task<IActionResult> CreateTicketAsync([FromBody] TicketDto ticketDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, result = "Invalid credentials" });

                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, result = "Invalid email or password" });

                var response = await _supportDeskService.CreateTicketAsync(ticketDto, email);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "InvalidUser" => Unauthorized(new { success = false, result = response.ErrorMessage }),
                        "InvalidCredential" => BadRequest(new { success = false, result = response.ErrorMessage }),
                        "ValidationError" => BadRequest(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, new { success = false, result = response.ErrorMessage })
                    };

                return StatusCode(StatusCodes.Status201Created, (new { success = response.Success, result = $"new ticket created." }));
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTicketAsync()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, result = "Invalid email or password" });


                var response = await _supportDeskService.GetAllTicketDetailsAsync(email);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "InvalidUser" => Unauthorized(new { success = false, result = response.ErrorMessage }),
                        "NotFound" => NotFound(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, new { success = false, result = response.ErrorMessage })
                    };

                return Ok(new { success = true, result = response.Data });
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }

        [HttpGet("ticketdetails/{ticketId:long}")]
        public async Task<IActionResult> GetTicketDetailsByTicketId([FromQuery] long ticketId)
        {
            try
            {
                if (ticketId == 0)
                    return BadRequest(new { success = false, result = "Ticket Id cann't be zero or negative" });

                var response = await _supportDeskService.GetTicketDetailsByIdAsync(ticketId);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "NotFound" => NotFound(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, new { success = false, result = response.ErrorMessage })
                    };

                return Ok(new { success = true, response = response.Data });
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }

        [HttpPut("updateticketstatus")]
        public async Task<IActionResult> UpdateTicketAsync([FromBody] TicketStatusUpdateRequest ticketStatusUpdateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, result = "Invalid credentials" });

                if (ticketStatusUpdateRequest.TicketId == 0)
                    return BadRequest(new { success = false, result = "Ticket Id should not be zero or negative." });

                var response = await _supportDeskService.UpdateTicketStatusByTicketIdAsync(ticketStatusUpdateRequest);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "NotFound" => NotFound(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, new { success = false, result = response.ErrorMessage })
                    };

                return Ok(new { success = true, result = response.Data });
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }

        [HttpPost("createcomment")]
        public async Task<IActionResult> CreateCommentAsync([FromBody] TicketCommentRequest ticketCommentRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, result = "Invalid credentials" });

                if (ticketCommentRequest.TicketId == 0)
                    return BadRequest(new { success = false, result = "Ticket Id should not be zero or negative." });

                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, result = "Invalid email or password" });

                var response = await _supportDeskService.CreateTicketCommentAsync(ticketCommentRequest, email);
                if (!response.Success)
                    return response.ErrorCode switch
                    {
                        "InvalidUser" => Unauthorized(new { success = false, result = response.ErrorMessage }),
                        "NotFound" => NotFound(new { success = false, result = response.ErrorMessage }),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, new { success = false, result = response.ErrorMessage })
                    };

                return StatusCode(StatusCodes.Status201Created, new { success = false, result = $"Comment created for TicketId: {ticketCommentRequest.TicketId}" });
            }
            catch
            {
                return BadRequest(new { success = false, result = "Internal Server Error" });
            }
        }

    }
}
