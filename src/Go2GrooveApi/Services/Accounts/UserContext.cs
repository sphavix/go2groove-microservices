using Go2GrooveApi.Domain.Dtos;
using System.Security.Claims;

namespace Go2GrooveApi.Services.Accounts
{
    public interface IUserContext
    {
        CurrentUserDto GetCurrentUser();
    }

    public class UserContext(IHttpContextAccessor _httpContext, ILogger<UserContext> logger) : IUserContext
    {
        public CurrentUserDto GetCurrentUser()
        {
            var httpContext = _httpContext.HttpContext;
            if(httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            var user = httpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Log all claims for debugging
            foreach (var claim in user.Claims)
            {
                logger.LogInformation("Claim Type: {ClaimType}, Claim Value: {ClaimValue}", claim.Type, claim.Value);
            }

            //var userId = user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
            //var userEmail = user.FindFirst(x => x.Type == ClaimTypes.Email)!.Value;
            //var roles = user.Claims.Where(x => x.Type == ClaimTypes.Role)!.Select(c => c.Value);

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
            var roles = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if(userId == null || userEmail == null)
            {
                throw new InvalidOperationException("Required claims (NameIdentifier or Email) are missing.");
            }

            return new CurrentUserDto(userId, userEmail, roles);
        }
    }
}
