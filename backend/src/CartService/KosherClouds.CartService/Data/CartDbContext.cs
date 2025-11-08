namespace KosherClouds.CartService.Data;

using Microsoft.EntityFrameworkCore;
using CartService.Entities;

    public class  CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
    {
         
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique(); 
                
                entity.HasMany(e => e.Items)
                      .WithOne(i => i.ShoppingCart)
                      .HasForeignKey(i => i.ShoppingCartId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.HasIndex(e => new { e.ShoppingCartId, e.ProductId }).IsUnique();
            });
        }
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is ShoppingCart cart)
                {
                    cart.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }