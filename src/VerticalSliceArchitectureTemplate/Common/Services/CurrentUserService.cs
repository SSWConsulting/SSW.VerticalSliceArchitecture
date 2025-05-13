using System.Security.Claims;
using VerticalSliceArchitectureTemplate.Common.Interfaces;

namespace VerticalSliceArchitectureTemplate.Common.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}