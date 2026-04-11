using System.Security.Claims;

namespace French_Learning_Platform.Security;

public static class ClaimsPrincipalExtensions
{
    public static int? GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(rawId, out var parsed) ? parsed : null;
    }
}
