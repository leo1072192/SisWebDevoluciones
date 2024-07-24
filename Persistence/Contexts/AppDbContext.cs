using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NpgsqlTypes;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }
        public DbSet<ReturnRequestEntity> ReturnRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReturnRequestEntity>()
    .Property(r => r.DocDate)
    .HasConversion(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
    );
            modelBuilder.Entity<Devolucion>()
                .Property(e => e.DocumentLines)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<DevolucionLinea>>(v))
                .HasColumnType("jsonb");

            modelBuilder.Entity<ReturnRequestEntity>()
                .Property(e => e.DocumentLines)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<DocumentLineEntity>>(v))
                .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
        }
    }
}
