using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>,
        UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserRole>(userRole => 
            {
                // Generate a new key for UserRole by combining UserId and RoleId
                // refer to UserRole.cs which has only the 2 related properties
                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

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
            
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
        }
    }
}