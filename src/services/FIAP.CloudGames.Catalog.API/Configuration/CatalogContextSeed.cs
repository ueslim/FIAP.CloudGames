using FIAP.CloudGames.Catalog.API.Data;
using FIAP.CloudGames.Catalog.API.Models;

namespace FIAP.CloudGames.Catalog.API.Configuration
{
    public class CatalogContextSeed
    {
        public static async Task EnsureSeedProducts(CatalogContext context)
        {
            if (context.Products.Any())
                return;

            var products = new List<Product>
            {
                new Product
                {
                    Name = "The Legend of Zelda: Breath of the Wild",
                    Description = "An open-world adventure game where players explore the vast kingdom of Hyrule.",
                    Value = 59.99m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/0/0b/The_Legend_of_Zelda_Breath_of_the_Wild.jpg",
                    StockQuantity = 100,
                    Active = true,
                    DateRegister = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Red Dead Redemption 2",
                    Description = "An epic tale of life in America's unforgiving heartland.",
                    Value = 59.99m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/4/44/Red_Dead_Redemption_II.jpg",
                    StockQuantity = 100,
                    Active = true,
                    DateRegister = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Minecraft",
                    Description = "A sandbox game where players can build and explore infinite worlds.",
                    Value = 26.95m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/5/51/Minecraft_cover.png",
                    StockQuantity = 100,
                    Active = true,
                    DateRegister = DateTime.UtcNow
                },
                new Product
                {
                    Name = "God of War",
                    Description = "Kratos embarks on a journey with his son Atreus in the Norse realms.",
                    Value = 49.99m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/a/a7/God_of_War_4_cover.jpg",
                    StockQuantity = 100,
                    Active = true,
                    DateRegister = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Super Mario Odyssey",
                    Description = "Mario embarks on a globe-trotting adventure to rescue Princess Peach.",
                    Value = 59.99m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/8/8d/Super_Mario_Odyssey.jpg",
                    StockQuantity = 100,
                    Active = true,
                    DateRegister = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Cyberpunk 2077",
                    Description = "An open-world RPG set in Night City, a megalopolis obsessed with power and glamour.",
                    Value = 39.99m,
                    Image = "https://upload.wikimedia.org/wikipedia/en/9/9f/Cyberpunk_2077_box_art.jpg",
                    StockQuantity = 50,
                    Active = false,
                    DateRegister = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}