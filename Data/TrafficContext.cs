using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class TrafficContext(DbContextOptions<TrafficContext> options) : IdentityDbContext<Traffic.Data.ApplicationUser>(options)
{
}
