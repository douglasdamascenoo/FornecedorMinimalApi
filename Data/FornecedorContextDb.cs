using Microsoft.EntityFrameworkCore;
using minimalFornecedor.Models;

namespace minimalFornecedor.Data
{
    public class FornecedorContextDb : DbContext
    {
        public FornecedorContextDb(DbContextOptions<FornecedorContextDb> options) : base(options) { }

        public DbSet<Fornecedor>? Fornecedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fornecedor>()
            .HasKey(p => p.Id);

            modelBuilder.Entity<Fornecedor>()
            .Property(p => p.Nome)
            .IsRequired()
            .HasColumnType("varchar(200)");

            modelBuilder.Entity<Fornecedor>()
            .Property(p => p.Documento)
            .IsRequired()
            .HasColumnType("varchar(14)");

            modelBuilder.Entity<Fornecedor>()
            .ToTable("Fornecedores");

            base.OnModelCreating(modelBuilder);
        }
    }
}