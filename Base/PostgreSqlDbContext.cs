using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Pawlio.Controllers;
using Pawlio.Models;

namespace Pawlio;

public class PostgreSqlDbContext : DbContext
{
    public static string ConnectionString = "";

    //public DbSet<City> Cities { get; set; } = null!;
    //public DbSet<District> Districties { get; set; } = null!;
    public DbSet<Symptom> Symptoms { get; set; } = null!;

    //public DbSet<AnimalCategory> AnimalCategories { get; set; }
    //public DbSet<AnimalType> AnimalTypes { get; set; }
    //public DbSet<AnimalBreed> AnimalBreeds { get; set; }
    //public DbSet<AnimalColor> AnimalColors { get; set; }

    public DbSet<Image> Images { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Firm> Firms { get; set; } = null!;
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<UserFirm> UserFirms { get; set; } = null!;
    public DbSet<UserFirmsBranch> UserFirmsBranches { get; set; } = null!;
    public DbSet<Definition> Definitions { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Animal> Animals { get; set; } = null!;
    public DbSet<AnimalAccounting> AnimalAccountings { get; set; } = null!;
    public DbSet<AnimalAppointment> AnimalAppointments { get; set; } = null!;
    public DbSet<AnimalImage> AnimalImages { get; set; } = null!;
    public DbSet<AnimalWeight> AnimalWeights { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Basket> Baskets { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductAmount> ProductAmounts { get; set; } = null!;
    public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Accounting> Accountings { get; set; } = null!;
    public DbSet<Balance> Balances { get; set; } = null!;
    public DbSet<ExaminationSymptom> ExaminationSymptoms { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;

    // Backup Tables
    //public DbSet<_User> _Users { get; set; } = null!;
    //public DbSet<_Firm> _Firms { get; set; } = null!;
    //public DbSet<_Branch> _Branches { get; set; } = null!;
    //public DbSet<_UserFirm> _UserFirms { get; set; } = null!;
    //public DbSet<_UserFirmsBranch> _UserFirmsBranches { get; set; } = null!;
    //public DbSet<_Definition> _Definitions { get; set; } = null!;
    //public DbSet<_Customer> _Customers { get; set; } = null!;
    //public DbSet<_Animal> _Animals { get; set; } = null!;
    //public DbSet<_AnimalWeight> _AnimalWeights { get; set; } = null!;
    //public DbSet<_Supplier> _Suppliers { get; set; } = null!;
    //public DbSet<_Basket> _Baskets { get; set; } = null!;
    //public DbSet<_Product> _Products { get; set; } = null!;
    //public DbSet<_Payment> _Payments { get; set; } = null!;
    //public DbSet<_Appointment> _Appointments { get; set; } = null!;
    //public DbSet<_Accounting> _Accountings { get; set; } = null!;
    //public DbSet<_Balance> _Balances { get; set; } = null!;
    //public DbSet<_Device> _Devices { get; set; } = null!;

    public PostgreSqlDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ürün adedi şubelere göre tutuluyor
        modelBuilder.Entity<ProductAmount>().HasKey(pa => new { pa.BranchId, pa.ProductId });

        modelBuilder.Entity<Definition>().Property(d => d.ValueType).HasColumnType("smallint").HasDefaultValue(DefinitionValueType.None);

        modelBuilder.Entity<Product>().Property(d => d.CriticalAmount).HasDefaultValue(0);
        modelBuilder.Entity<Product>().Property(d => d.CriticalAmountAlert).HasDefaultValue(false);

        modelBuilder.Entity<Accounting>().Property(d => d.CreaterId).HasColumnName("CreaterId");
        modelBuilder.Entity<Accounting>().Property(d => d.BasketId).HasDefaultValue(1);

        modelBuilder.Entity<Payment>().Property(p => p.BasketId).HasDefaultValue(1);

        // Default value Pawlio
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ModelBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ModelBase.Flavor))
                    .HasDefaultValue(Flavor.Pawlio)
                    .HasSentinel(Flavor.Pawlio); ;
            }
        }
    }

    public int Save(ApiController controller)
    {
        OnBeforeSaving(controller?.GetUser());
        return SaveChanges();
    }

    public async Task<int> SaveAsync(ApiController controller)
    {
        OnBeforeSaving(controller?.GetUser());
        return await SaveChangesAsync();
    }

    private void OnBeforeSaving(TokenUser? user)
    {
        if (user == null) return;

        var entries = ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.Entity is ModelBase m)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        m.Updated = DateTimeOffset.UtcNow;
                        m.UpdaterId = user.Id;
                        break;

                    case EntityState.Added:
                        //m.Created = DateTime.Now;
                        m.CreaterId = user.Id;
                        break;

                    case EntityState.Deleted:
                        break;
                }
            }
        }
    }
}
