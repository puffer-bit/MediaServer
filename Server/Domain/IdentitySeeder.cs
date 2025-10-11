using Microsoft.AspNetCore.Identity;

namespace Server.Domain;

public class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        const string userName = "admin";
        const string password = "Admin123";

        if (await userManager.FindByEmailAsync(userName) == null)
        {
            var user = new IdentityUser
            {
                UserName = userName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                Console.WriteLine("Пользователь создан.");
            }
            else
            {
                Console.WriteLine("Ошибки при создании:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"- {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("Пользователь уже существует.");
        }
    }}