using Microsoft.EntityFrameworkCore;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data
{
    public class SummitDbContext: DbContext
    {
        public SummitDbContext(DbContextOptions<SummitDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Category>()
                .HasMany(cat => cat.Items)
                .WithOne(item => item.Category)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder .Entity<Item>()
                .HasMany(item => item.ItemImages)
                .WithOne(image => image.Item)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
