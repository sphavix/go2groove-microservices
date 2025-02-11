using System.Security.Claims;

namespace Go2GrooveApi.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot find the username!");
        }

        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            return Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot find the userId"));
        }
    }
}
