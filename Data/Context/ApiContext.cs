using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class ApiContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public ApiContext(DbContextOptions<ApiContext> optionsBuilder) : base(optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;initial catalog=DiscordAppDb;integrated security=true");
        }


        public DbSet<Channel> Channels { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FriendShip> FriendShips { get; set; }

        public DbSet<Server> Servers { get; set; }
        public DbSet<UserServer> UserServers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Message tablosundaki alıcı ve gönderici çakışmasın diye
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendShip>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendShip>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Channel -> Server ilişkisi (Bir sunucunun birden fazla kanalı olur)
            modelBuilder.Entity<Channel>()
                .HasOne(c => c.Server)
                .WithMany(s => s.Channels)
                .HasForeignKey(c => c.ServerId)
                .OnDelete(DeleteBehavior.Cascade); // Sunucu silinirse kanalları da silinsin

            // UserServer -> Server ilişkisi
            modelBuilder.Entity<UserServer>()
                .HasOne(us => us.Server)
                .WithMany(s => s.UserServers)
                .HasForeignKey(us => us.ServerId)
                .OnDelete(DeleteBehavior.Cascade); // Sunucu silinirse üyelikler de silinsin

            // UserServer -> AppUser ilişkisi
            modelBuilder.Entity<UserServer>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }
    }
}