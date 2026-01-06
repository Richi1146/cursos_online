using System.ComponentModel.DataAnnotations;

namespace OnlineCourses.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }

        public static ApiResult<T> Ok(T data) => new ApiResult<T> { Success = true, Data = data };
        public static ApiResult<T> Fail(string message) => new ApiResult<T> { Success = false, ErrorMessage = message };
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
