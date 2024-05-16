using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DAL.Data
{
    public class SummitDbContext: IdentityDbContext
    {
        public SummitDbContext(DbContextOptions<SummitDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FileUploadTracker> FileUploads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Category>()
                .HasMany(cat => cat.Items)
                .WithOne(item => item.Category)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder .Entity<Item>()
                .HasMany(item => item.ItemImages)
                .WithOne(image => image.Item)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Name = AppRoles.ADMIN,
                    NormalizedName = AppRoles.ADMIN.ToUpper(),
                });
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Name = AppRoles.GENERAL,
                    NormalizedName= AppRoles.GENERAL.ToUpper(),
                });
        }
    }
}
