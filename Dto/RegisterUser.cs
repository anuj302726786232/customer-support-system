using SupportDeskAPI.Entity;
using System.ComponentModel.DataAnnotations;

namespace SupportDeskAPI.Dto
{
    public class RegisterUser
    {
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one letter and one number")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "User role is required")]
        public UserRole? UserRole { get; set; }
    }


    public class SupportDeskResponse<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public T? Data { get; set; }

        public static SupportDeskResponse<T> Fail(string errorCode, string message)
        {
            return new SupportDeskResponse<T>
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = message,
                Data = default
            };
        }

        public static SupportDeskResponse<T> Ok(T? data)
        {
            return new SupportDeskResponse<T>
            {
                Success = true,
                Data = data,
                ErrorCode = null,
                ErrorMessage = null
            };
        }
    }
}
