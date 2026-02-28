using FinanceTracking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracking.API.Data;

public class FinanceDbContext : DbContext
{
      public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

      public DbSet<AppUser> Users { get; set; }
      public DbSet<Role> Roles { get; set; }
      public DbSet<Group> Groups { get; set; }
      public DbSet<GroupInvitation> GroupInvitations { get; set; }
      public DbSet<GroupMember> GroupMembers { get; set; }
      public DbSet<GroupMemberHistory> GroupMemberHistories { get; set; }
      public DbSet<Category> Categories { get; set; }
      public DbSet<Seller> Sellers { get; set; }
      public DbSet<ProductData> ProductData { get; set; }
      public DbSet<ProductDataCategory> ProductDataCategories { get; set; }
      public DbSet<Receipt> Receipts { get; set; }
      public DbSet<ProductEntry> ProductEntries { get; set; }
      public DbSet<BudgetGoal> BudgetGoals { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);

            // 1. AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                  entity.HasKey(e => e.Id);
            });

            // 2. Groups
            modelBuilder.Entity<Group>(entity =>
            {
                  entity.HasOne(g => g.Owner)
                        .WithMany(u => u.OwnedGroups)
                        .HasForeignKey(g => g.OwnerId)
                        .OnDelete(DeleteBehavior.SetNull);
            });

            // 3. GroupMembers (Composite PK)
            modelBuilder.Entity<GroupMember>(entity =>
            {
                  entity.HasKey(e => new { e.UserId, e.GroupId });
                  
                  entity.HasOne(gm => gm.User).WithMany(u => u.GroupMemberships)
                        .HasForeignKey(gm => gm.UserId).OnDelete(DeleteBehavior.Cascade);
                        
                  entity.HasOne(gm => gm.Group).WithMany(g => g.Members)
                        .HasForeignKey(gm => gm.GroupId).OnDelete(DeleteBehavior.Cascade);
                        
                  entity.HasOne(gm => gm.Role).WithMany(r => r.GroupMembers)
                        .HasForeignKey(gm => gm.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            // 4. GroupMemberHistory (Composite PK with Auto-Increment Id)
            modelBuilder.Entity<GroupMemberHistory>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(h => h.Group).WithMany()
                        .HasForeignKey(h => h.GroupId).OnDelete(DeleteBehavior.Cascade);
                        
                  entity.HasOne(h => h.User).WithMany(u => u.HistoryActions)
                        .HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.SetNull);
                        
                  entity.HasOne(h => h.ChangedByUser).WithMany(u => u.HistoryChangesMade)
                        .HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.SetNull);
            });

            // 5. Categories
            modelBuilder.Entity<Category>(entity =>
            {
                  entity.HasOne(c => c.Group).WithMany(g => g.Categories)
                        .HasForeignKey(c => c.GroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Sellers (Composite PK)
            modelBuilder.Entity<Seller>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(s => s.Group).WithMany(g => g.Sellers)
                        .HasForeignKey(s => s.GroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Receipts (Composite PK & Composite FKs)
            modelBuilder.Entity<Receipt>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(r => r.Group).WithMany(g => g.Receipts)
                        .HasForeignKey(r => r.GroupId).OnDelete(DeleteBehavior.Cascade);
                        
                  entity.HasOne(r => r.CreatedByUser).WithMany(u => u.CreatedReceipts)
                        .HasForeignKey(r => r.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
                        
                  // Matches foreign key: FOREIGN KEY (seller_id, group_id) REFERENCES sellers(id, group_id)
                  entity.Property(r => r.SellerId).IsRequired();
                  entity.HasOne(r => r.Seller).WithMany(s => s.Receipts)
                        .HasForeignKey(r => new { r.SellerId, r.GroupId })
                        .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. ProductData (Composite PK)
            modelBuilder.Entity<ProductData>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(p => p.Group).WithMany(g => g.Products)
                        .HasForeignKey(p => p.GroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // 9. ProductDataCategories (Tri-Composite PK & Composite FKs)
            modelBuilder.Entity<ProductDataCategory>(entity =>
            {
                  entity.HasKey(e => new { e.ProductDataId, e.CategoryId, e.GroupId });

                  entity.HasOne(pdc => pdc.ProductData).WithMany(pd => pd.ProductDataCategories)
                        .HasForeignKey(pdc => new { pdc.ProductDataId, pdc.GroupId })
                        .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(pdc => pdc.Category).WithMany(c => c.ProductDataCategories)
                        .HasForeignKey(pdc => pdc.CategoryId)
                        .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(pdc => pdc.Group).WithMany()
                        .HasForeignKey(pdc => pdc.GroupId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            // 10. ProductEntries (Composite PK & Composite FKs)
            modelBuilder.Entity<ProductEntry>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(pe => pe.Receipt).WithMany(r => r.ProductEntries)
                        .HasForeignKey(pe => new { pe.ReceiptId, pe.GroupId })
                        .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(pe => pe.ProductData).WithMany(pd => pd.ProductEntries)
                        .HasForeignKey(pe => new { pe.ProductDataId, pe.GroupId })
                        .OnDelete(DeleteBehavior.Cascade);
            });

            // 11. BudgetGoals (Composite PK)
            modelBuilder.Entity<BudgetGoal>(entity =>
            {
                  entity.HasKey(e => new { e.Id, e.GroupId });
                  entity.Property(e => e.Id).ValueGeneratedOnAdd();

                  entity.HasOne(bg => bg.Group).WithMany(g => g.BudgetGoals)
                        .HasForeignKey(bg => bg.GroupId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            // 12. GroupInvitations
            modelBuilder.Entity<GroupInvitation>(entity =>
            {
                  entity.HasKey(e => e.Id);

                  // Group relationship
                  entity.HasOne(i => i.Group)
                        .WithMany(g => g.Invitations)
                        .HasForeignKey(i => i.GroupId)
                        .OnDelete(DeleteBehavior.Cascade);

                  // User who sent the invite
                  entity.HasOne(i => i.InvitedByUser)
                        .WithMany(u => u.SentInvitations)
                        .HasForeignKey(i => i.InvitedByUserId)
                        .OnDelete(DeleteBehavior.Restrict);

                  // User receiving the invite
                  entity.HasOne(i => i.TargetUser)
                        .WithMany(u => u.ReceivedInvitations)
                        .HasForeignKey(i => i.TargetUserId)
                        .OnDelete(DeleteBehavior.Cascade); 
            });

            // 13. Seed Roles
            modelBuilder.Entity<Role>().HasData(
                  new Role { Id = GroupRole.Owner, Name = GroupRole.Owner.ToString() },
                  new Role { Id = GroupRole.Admin, Name = GroupRole.Admin.ToString() },
                  new Role { Id = GroupRole.Member, Name = GroupRole.Member.ToString() }
            );
      }

      public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      {
            var deletingEntries = ChangeTracker.Entries<ProductEntry>()
                  .Where(e => e.State == EntityState.Deleted)
                  .Select(e => e.Entity)
                  .ToList();

            foreach (var entry in deletingEntries)
            {
                  bool hasOtherEntries = await ProductEntries
                        .AnyAsync(
                              pe => pe.ProductDataId == entry.ProductDataId 
                                    && pe.GroupId == entry.GroupId 
                                    && pe.Id != entry.Id, 
                              cancellationToken
                        );

                  if (!hasOtherEntries)
                  {
                        var orphanedData = await ProductData.FindAsync(
                              new object[] { entry.ProductDataId, entry.GroupId }, 
                              cancellationToken
                        );
                        
                        if (orphanedData != null)
                        {
                              ProductData.Remove(orphanedData);
                        }
                  }
            }

            return await base.SaveChangesAsync(cancellationToken);
      }
}