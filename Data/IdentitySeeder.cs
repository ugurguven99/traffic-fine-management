
using Microsoft.AspNetCore.Identity;

namespace Traffic.Data
{
    public static class IdentitySeeder
    {
        // Admin, Manager ve Finance rollerini oluşturur.
        public static async Task SeedRolesAsync(
            IServiceProvider services)
        {
            var roleManager = services
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Admin",
                "Manager",
                "Finance"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager
                        .CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ",
                            result.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(errors);
                    }
                }
            }
        }

        // Mevcut kullanıcıya Admin rolünü atar.
        public static async Task SeedFirstAdminAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            var adminEmail = configuration["BootstrapAdmin:Email"];

            if (string.IsNullOrWhiteSpace(adminEmail))
                return;

            var userManager = services
                .GetRequiredService<UserManager<IdentityUser>>();

            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Admin olarak atanacak kullanıcı bulunamadı.");
            }

            if (await userManager.IsInRoleAsync(user, "Admin"))
                return;

            var result = await userManager
                .AddToRoleAsync(user, "Admin");

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }
        }
    }
}
