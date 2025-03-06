using DevExpress.ExpressApp.EFCore.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XAFVectorSearch.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891/core-prerequisites-for-design-time-model-editor-with-entity-framework-core-data-model.
public class XAFVectorSearchContextInitializer : DbContextTypesInfoInitializerBase
{
    protected override DbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<XAFVectorSearchDBContext>()
            .UseSqlServer(";")//.UseSqlite("; ") wrong for a solution with SqLite, see https://isc.devexpress.com/internal/ticket/details/t1240173
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new XAFVectorSearchDBContext(optionsBuilder.Options);
    }
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class XAFVectorSearchDesignTimeDbContextFactory : IDesignTimeDbContextFactory<XAFVectorSearchDBContext>
{
    public XAFVectorSearchDBContext CreateDbContext(string[] args)
    {
        //throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
        var optionsBuilder = new DbContextOptionsBuilder<XAFVectorSearchDBContext>()
        .UseSqlServer(";")
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies();
        return new XAFVectorSearchDBContext(optionsBuilder.Options);
    }
}
