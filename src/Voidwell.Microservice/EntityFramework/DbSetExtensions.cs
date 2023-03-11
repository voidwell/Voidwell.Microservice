using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Voidwell.Microservice.EntityFramework
{
    public static class DbSetExtensions
    {
        public static async Task<TEntity> UpsertAsync<TEntity>(this DbContext ctx, TEntity entity)
            where TEntity : class
        {
            await ctx.Upsert(entity)
                .RunAsync();

            return entity;
        }

        public static async Task<IEnumerable<TEntity>> UpsertAsync<TEntity>(this DbContext ctx, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            await ctx.UpsertRange(entities)
                .RunAsync();

            return entities;
        }

        //
        // Summary:
        //     If a match is found, only update properties that are not set to null in the new passed entity
        public static async Task<TEntity> UpsertWithoutNullPropertiesAsync<TEntity>(this DbContext dbContext, TEntity entity)
            where TEntity : class
        {
            var result = await dbContext.UpsertRangeWithoutNullPropertiesAsync(new[] { entity });
            return result.FirstOrDefault();
        }

        //
        // Summary:
        //     If a match is found, only update properties that are not set to null in the new passed entity
        public static async Task<IEnumerable<TEntity>> UpsertRangeWithoutNullPropertiesAsync<TEntity>(this DbContext dbContext, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var keyProps = GetKeyProperties<TEntity>(dbContext);
            var exprCtr = GetKeyConstructor(keyProps);

            var parameter = Expression.Parameter(typeof(TEntity), "e");

            var predicateExpression = GetPredicateExpression(parameter, exprCtr, keyProps, entities.ToArray());

            var dbSet = dbContext.Set<TEntity>();
            var storedEntities = await dbSet.Where(predicateExpression).ToListAsync();

            var result = new List<TEntity>();
            var createdEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                var predExpr = GetPredicateExpression(parameter, exprCtr, keyProps, entity).Compile();
                var storeEntity = storedEntities.FirstOrDefault(predExpr);
                if (storeEntity == null)
                {
                    createdEntities.Add(entity);
                }
                else
                {
                    var preparedEntity = PrepareEntityUpdate(dbSet, storeEntity, entity);
                    result.Add(preparedEntity);
                }
            }

            if (createdEntities.Any())
            {
                await dbSet.AddRangeAsync(createdEntities);
                result.AddRange(createdEntities);
            }

            await dbContext.SaveChangesAsync();

            return result;
        }

        private static ConstructorInfo GetKeyConstructor(IReadOnlyList<IProperty> keyProps)
        {
            var propertyTypes = keyProps.Select(a => a.ClrType).ToArray();
            var tupleType = typeof(Tuple).Assembly.GetType("System.Tuple`" + propertyTypes.Length);
            return tupleType.MakeGenericType(propertyTypes).GetConstructor(propertyTypes);
        }

        private static IReadOnlyList<IProperty> GetKeyProperties<TEntity>(DbContext dbContext)
            where TEntity : class
        {
            return dbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;
        }

        private static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods()
            .FirstOrDefault(mi => mi.Name == "Contains" && mi.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(object));

        private static Expression<Func<TEntity, bool>> GetPredicateExpression<TEntity>(ParameterExpression paramExpr, ConstructorInfo exprCtr, IReadOnlyList<IProperty> keyProps, params TEntity[] entities)
            where TEntity : class
        {
            Expression body;

            if (entities.Length > 1)
            {
                var keyValues = entities.Select(entity => exprCtr.Invoke(keyProps.Select(a => a.PropertyInfo.GetValue(entity)).ToArray()));

                body = Expression.Call(null, ContainsMethod,
                    Expression.Constant(keyValues),
                    Expression.New(exprCtr, keyProps.Select(k => Expression.Property(paramExpr, k.PropertyInfo))));
            }
            else
            {
                body = keyProps
                    .Select((p, i) => Expression.Equal(
                        Expression.Property(Expression.Constant(entities[0]), p.Name),
                        Expression.Property(paramExpr, p.Name)))
                    .Aggregate(Expression.AndAlso);
            }


            return Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
        }

        private static T PrepareEntityUpdate<T>(DbSet<T> dbSet, T target, T source) where T : class
        {
            foreach (var fromProp in typeof(T).GetProperties())
            {
                var toProp = typeof(T).GetProperty(fromProp.Name);
                var toValue = toProp.GetValue(source, null);
                if (toValue != null)
                {
                    fromProp.SetValue(target, toValue, null);
                }
            }

            dbSet.Update(target);
            return target;
        }
    }
}
