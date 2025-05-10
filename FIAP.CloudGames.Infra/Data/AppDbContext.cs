using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}
