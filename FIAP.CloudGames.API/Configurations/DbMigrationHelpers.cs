using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Infra;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.API.Configurations
{
    public static class DbMigrationHelpers
    {
        public static async Task EnsureSeedData(WebApplication serviceScope)
        {
            var services = serviceScope.Services.CreateScope().ServiceProvider;
            await EnsureSeedData(services);
        }

        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (env.IsDevelopment() || env.IsEnvironment("Docker"))
            {
                await context.Database.MigrateAsync();
                await EnsureSeedGames(context);
            }
        }

        private static async Task EnsureSeedGames(AppDbContext context)
        {
            // Only seed games if the table is empty
            if (context.Games.Any())
                return;

            var games = new List<Game>
        {
            new Game
            {
                Title = "The Legend of Zelda: Breath of the Wild",
                Description = "An open-world adventure game where players explore the vast kingdom of Hyrule.",
                Developer = "Nintendo",
                Publisher = "Nintendo",
                Genre = "Action-Adventure",
                ReleaseDate = new DateTime(2017, 03, 03),
                Price = 59.99m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/0/0b/The_Legend_of_Zelda_Breath_of_the_Wild.jpg",
                Tags = ["Adventure", "Open World", "Action"],
                IsActive = true
            },
            new Game
            {
                Title = "Red Dead Redemption 2",
                Description = "An epic tale of life in America's unforgiving heartland.",
                Developer = "Rockstar Games",
                Publisher = "Rockstar Games",
                Genre = "Action-Adventure",
                ReleaseDate = new DateTime(2018, 10, 26),
                Price = 59.99m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/4/44/Red_Dead_Redemption_II.jpg",
                Tags = ["Western", "Open World", "Story Rich"],
                IsActive = true
            },
            new Game
            {
                Title = "Minecraft",
                Description = "A sandbox game where players can build and explore infinite worlds.",
                Developer = "Mojang Studios",
                Publisher = "Mojang Studios",
                Genre = "Sandbox",
                ReleaseDate = new DateTime(2011, 11, 18),
                Price = 26.95m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/5/51/Minecraft_cover.png",
                Tags = ["Sandbox", "Survival", "Creative"],
                IsActive = true
            },
            new Game
            {
                Title = "God of War",
                Description = "Kratos embarks on a journey with his son Atreus in the Norse realms.",
                Developer = "Santa Monica Studio",
                Publisher = "Sony Interactive Entertainment",
                Genre = "Action-Adventure",
                ReleaseDate = new DateTime(2018, 04, 20),
                Price = 49.99m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/a/a7/God_of_War_4_cover.jpg",
                Tags = ["Action", "Mythology", "Story Rich"],
                IsActive = true
            },
            new Game
            {
                Title = "Super Mario Odyssey",
                Description = "Mario embarks on a globe-trotting adventure to rescue Princess Peach.",
                Developer = "Nintendo",
                Publisher = "Nintendo",
                Genre = "Platformer",
                ReleaseDate = new DateTime(2017, 10, 27),
                Price = 59.99m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/8/8d/Super_Mario_Odyssey.jpg",
                Tags = ["Platformer", "Adventure", "Family"],
                IsActive = true
            },
            // Additional inactive game example
            new Game
            {
                Title = "Cyberpunk 2077",
                Description = "An open-world RPG set in Night City, a megalopolis obsessed with power and glamour.",
                Developer = "CD Projekt Red",
                Publisher = "CD Projekt",
                Genre = "RPG",
                ReleaseDate = new DateTime(2020, 12, 10),
                Price = 39.99m,
                CoverImageUrl = "https://upload.wikimedia.org/wikipedia/en/9/9f/Cyberpunk_2077_box_art.jpg",
                Tags = ["RPG", "Open World", "Futuristic"],
                IsActive = false
            }
        };

            await context.Games.AddRangeAsync(games);
            await context.SaveChangesAsync();
        }
    }
}