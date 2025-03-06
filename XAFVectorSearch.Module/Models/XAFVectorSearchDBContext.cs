
#nullable disable
using System;
using System.Collections.Generic;
using DevExpress.ExpressApp.Design;
using DevExpress.Persistent.BaseImpl.EF;
using Microsoft.EntityFrameworkCore;

namespace XAFVectorSearch.Module.BusinessObjects;

[TypesInfoInitializer(typeof(XAFVectorSearchContextInitializer))]
public partial class XAFVectorSearchDBContext(DbContextOptions<XAFVectorSearchDBContext> options) : DbContext(options)
{
    public virtual DbSet<DocumentChunks> DocumentChunks { get; set; }

    public virtual DbSet<Documents> Documents { get; set; }

    public virtual DbSet<FileData> FileData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<DocumentChunks>(entity =>
        {
            entity.Property(e => e.ID).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Embedding)
                 .HasColumnType("vector(1536)")
                 .IsRequired();
                

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentChunks)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_DocumentChunks_Documents");
        });

        modelBuilder.Entity<Documents>(entity =>
        {
            entity.Property(e => e.ID).HasDefaultValueSql("(newsequentialid())");
           
        });

        



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}