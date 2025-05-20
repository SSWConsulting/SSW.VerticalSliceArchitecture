using SSW.VerticalSliceArchitecture.Common.Interfaces;
using System.Security.Claims;

namespace SSW.VerticalSliceArchitecture.Common.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}