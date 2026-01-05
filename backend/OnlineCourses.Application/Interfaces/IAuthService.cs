using System.Threading.Tasks;
using OnlineCourses.Application.DTOs;

namespace OnlineCourses.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResult<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}
