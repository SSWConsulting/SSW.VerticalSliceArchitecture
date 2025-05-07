using System.Security.Claims;

namespace VerticalSliceArchitectureTemplate.Common.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}