using Microsoft.AspNetCore.Identity;

namespace Go2GrooveApi.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
