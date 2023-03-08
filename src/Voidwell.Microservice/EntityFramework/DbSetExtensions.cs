using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        //
        // Summary:
        //     If a match is found, only update properties that are not set to null in the new passed entity
        public static async Task<TEntity> UpsertWithoutNullPropertiesAsync<TEntity>(this DbSet<TEntity> dbSet, TEntity entity, Expression<Func<TEntity, bool>> searchPredicate)
            where TEntity : class
        {
            var storeEntity = await dbSet.Where(searchPredicate).FirstOrDefaultAsync();

            if (storeEntity == null)
            {
                await dbSet.AddAsync(entity);
                return entity;
            }
            else
            {
                var preparedEntity = PrepareEntityUpdate(dbSet, storeEntity, entity);
                return preparedEntity;
            }
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
        public static async Task<IEnumerable<TEntity>> UpsertWithoutNullPropertiesAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities, Expression<Func<TEntity, bool>> searchPredicate, Func<TEntity, TEntity, bool> matchPredicate)
            where TEntity : class
        {
            var result = new List<TEntity>();
            var createdEntities = new List<TEntity>();

            var storedEntities = await (searchPredicate != null ? dbSet.Where(searchPredicate) : dbSet).ToListAsync();

            foreach (var entity in entities)
            {
                var storeEntity = storedEntities.FirstOrDefault(storedEntity => matchPredicate(storedEntity, entity));
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

            return result;
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
