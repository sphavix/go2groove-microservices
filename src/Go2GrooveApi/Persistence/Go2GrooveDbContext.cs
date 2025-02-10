using Go2GrooveApi.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Go2GrooveApi.Persistence
{
    public class Go2GrooveDbContext(DbContextOptions<Go2GrooveDbContext> options) 
        : IdentityDbContext<ApplicationUser>(options)
    {
    }
}
