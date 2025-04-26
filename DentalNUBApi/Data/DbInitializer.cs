using DentalNUB.Api.Entities;


namespace DentalNUB.Api.Data;

public class DbInitializer
{
    public static async Task Initialize(DentalNUBDbContext context)
    {
        if (await context.Database.EnsureCreatedAsync())
        {
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    FullName = "عبد الله محمد",
                    Email = "abd@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("abd1234"),
                 
                    Role = "Admin"
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
