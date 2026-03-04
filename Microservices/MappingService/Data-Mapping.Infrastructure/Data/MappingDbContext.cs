using Data_Mapping.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Infrastructure.Data
{
    public partial class MappingDbContext : DbContext
    {
        public MappingDbContext()
        {
        }

        public MappingDbContext(DbContextOptions<MappingDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClientSchemaVector> ClientSchemaVectors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientSchemaVector>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("client_schema_vectors_pkey");

                entity.ToTable("client_schema_vectors");

                entity.HasIndex(e => new { e.CompanyId, e.TableName }, "uk_company_table").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CompanyId)
                    .HasMaxLength(255)
                    .HasColumnName("company_id");
                entity.Property(e => e.TableDescription).HasColumnName("table_description");
                entity.Property(e => e.TableName)
                    .HasMaxLength(255)
                    .HasColumnName("table_name");
                entity.Property(e => e.TableRelations)
                    .HasColumnType("jsonb")
                    .HasColumnName("table_relations");
                entity.Property(e => e.TeamName)
                    .HasDefaultValueSql("'{}'::text[]")
                    .HasColumnName("team_name");
            });
            modelBuilder.HasSequence<int>("seq_schema_version", "graphql").IsCyclic();

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}