using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Core.Models;

namespace Notification.Core.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) 
            : base(options) { }

        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Core.Models.NotificationModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired(); 
                entity.Property(e => e.ReceiverId).IsRequired();
                entity.HasIndex(e => e.ReceiverId);
            });

            modelBuilder.Entity<Core.Models.UserDeviceToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DeviceToken).IsRequired();
                entity.HasIndex(e => e.UserId);
            });
        }
    }
}
