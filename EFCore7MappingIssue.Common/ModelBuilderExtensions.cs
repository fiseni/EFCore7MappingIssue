using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore7MappingIssue.Common;

public static class ModelBuilderExtensions
{
    public static void ConfigureCustomRules(this ModelBuilder modelBuilder, DbContext dbContext)
    {
        // The order is important

        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(dbContext.GetType().Assembly);

        modelBuilder.ConfigureTableAndColumnNames(dbContext);

        modelBuilder.ConfigureSoftDelete();
    }

    private static void ConfigureTableAndColumnNames(this ModelBuilder modelBuilder, DbContext dbContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
            {
                ConfigureOwnedTypeColumnNames(entityType);
            }
            else
            {
                ConfigureTableNames(entityType, dbContext);
            }
        }
    }

    private static void ConfigureTableNames(IMutableEntityType entityType, DbContext dbContext)
    {
        var dbSetNames = dbContext.GetType().GetProperties()
            .Where(x => x.PropertyType.IsGenericType && typeof(DbSet<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition()))
            .Select(x => x.Name)
            .ToList();

        var tableName = entityType.GetTableName();
        var defaultTableName = entityType.GetDefaultTableName();

        if (tableName is null
            || defaultTableName is null
            || tableName.Equals(defaultTableName)) return;

        if (dbSetNames.Find(x => x.Equals(tableName)) is not null)
        {
            entityType.SetTableName(entityType.DisplayName());
        }
    }

    private static void ConfigureOwnedTypeColumnNames(IMutableEntityType entityType)
    {
        var ownership = entityType.FindOwnership();

        if (ownership is null) return;

        ownership.IsRequiredDependent = true;

        var properties = entityType.GetProperties().Where(x => !x.IsShadowProperty());

        foreach (var property in properties)
        {
            var tableName = entityType.GetTableName();

            if (tableName is null) continue;

            var columnName = property.GetColumnName(StoreObjectIdentifier.Table(tableName, null));
            var columnNameDefault = property.GetDefaultColumnName(StoreObjectIdentifier.Table(tableName, null));

            if (columnName is null || columnNameDefault is null) continue;

            if (columnName.Equals(columnNameDefault))
            {
                var columnNameBase = property.GetColumnName();

                property.SetColumnName(columnNameBase);
            }
        }
    }

    private static void ConfigureSoftDelete(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name, x => x.Property(nameof(ISoftDelete.IsDeleted)));
                entityType.AddSoftDeleteQueryFilter();
            }
        }
    }

    public static void ConfigureSoftDeleteByPropertyName(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isDeletedProperty = entityType.FindProperty("IsDeleted");
            if (isDeletedProperty is not null && isDeletedProperty.PropertyInfo is not null && isDeletedProperty.ClrType == typeof(bool))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, isDeletedProperty.PropertyInfo),
                        Expression.Constant(false, typeof(bool))
                    )
                    , parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }

    private static void AddSoftDeleteQueryFilter(this IMutableEntityType entityData)
    {
        var methodToCall = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(GetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityData.ClrType);

        var filter = methodToCall.Invoke(null, Array.Empty<object>());

        entityData.SetQueryFilter((LambdaExpression?)filter);
        entityData.AddIndex(entityData.FindProperty(nameof(ISoftDelete.IsDeleted))!);
    }

    private static LambdaExpression GetSoftDeleteFilter<TEntity>() where TEntity : class, ISoftDelete
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}
