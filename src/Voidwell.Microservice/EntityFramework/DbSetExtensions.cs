using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        public static async Task<IEnumerable<TEntity>> UpsertWithoutNullPropertiesAsync<TEntity>(this DbContext ctx, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            await ctx.UpsertRange(entities)
                .WhenMatched((existingRec, newRec) => AssignNonNullProperties(existingRec, newRec))
                .RunAsync();

            return entities;
        }

        private static TEntity AssignNonNullProperties<TEntity>(TEntity target, TEntity source) where TEntity : class
        {
            foreach (var fromProp in typeof(TEntity).GetProperties())
            {
                var toProp = typeof(TEntity).GetProperty(fromProp.Name);
                var toValue = toProp.GetValue(source, null);
                if (toValue != null)
                {
                    fromProp.SetValue(target, toValue, null);
                }
            }

            return target;
        }
    }
}
