using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // k means combination(actually k is an instance of class Like)
            // which helps HasKey() form the primary key, so table Likes
            // will have a primary key although we don't create this pro-
            // perty explicitly in Like.cs.
            // builder.Entity<T>() returns an object that can be used to
            // configure a given entity type in the model. If the entity
            // type is not already part of the model, it will be added to
            // the model.
            builder.Entity<Like>().HasKey(k => new {k.LikerId, k.LikeeId});

            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)     // generate FK1(Foreign Key)
                .OnDelete(DeleteBehavior.Restrict);// Rule on FK1
            
            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)     // generate FK2(Foreign Key)
                .OnDelete(DeleteBehavior.Restrict);// Rule on FK2, yes this table has 2 FKs!
        }
    }
}