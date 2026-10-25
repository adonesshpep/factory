using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace neotonic
{
    public static class DeletionLogic
    {
        public static async Task<object> GetDeleteConflictsAsync<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var entityType = typeof(TEntity);

            var pkProp = entityType.GetProperty("Id");
            if (pkProp == null) return "Unable to determine dynamic conflicts: 'Id' property not found.";
            var entityIdValue = pkProp.GetValue(entity);

            var referencingForeignKeys = context.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.PrincipalEntityType.ClrType == entityType);

            var allConflicts = new List<object>();

            var genericSetMethod = typeof(DbContext).GetMethods()
                .First(m => m.Name == "Set" && m.IsGenericMethod && m.GetParameters().Length == 0);

            var toListAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                         && m.GetParameters().Length == 2);

            foreach (var fk in referencingForeignKeys)
            {
                var dependentEntityType = fk.DeclaringEntityType;

                string fkPropertyName = fk.Properties.First().Name;
                string pkPropertyName = dependentEntityType.FindPrimaryKey()?.Properties.First().Name ?? "Id";
                string tableName = dependentEntityType.GetTableName() ?? dependentEntityType.Name;

                var clrType = dependentEntityType.ClrType;
                var parameter = Expression.Parameter(clrType, "x");

                // Build the comparison with the FK property's OWN type (int? for an
                // optional FK like Step.BoxPathId, plain int for a required one), so
                // the constant always matches — this is what was missing before.
                var fkProperty = clrType.GetProperty(fkPropertyName);
                var fkPropertyType = fkProperty?.PropertyType ?? typeof(object);

                var left = Expression.Property(parameter, fkPropertyName);
                Expression right = Expression.Constant(entityIdValue, fkPropertyType);
                var body = Expression.Equal(left, right);
                var lambda = Expression.Lambda(body, parameter);

                var dynamicSetMethod = genericSetMethod.MakeGenericMethod(clrType);
                var dbSetInstance = (IQueryable?)dynamicSetMethod.Invoke(context, null);
                if (dbSetInstance == null) continue;

                var query = dbSetInstance.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable), "Where", new Type[] { clrType },
                        dbSetInstance.Expression, Expression.Quote(lambda)
                    )
                );

                // Materialize asynchronously via reflection, since clrType is only
                // known at runtime — a plain foreach here would block the thread
                // on the DB call instead of awaiting it.
                var genericToListAsync = toListAsyncMethod.MakeGenericMethod(clrType);
                var task = (Task)genericToListAsync.Invoke(null, new object[] { query, CancellationToken.None })!;
                await task.ConfigureAwait(false);
                var records = (System.Collections.IEnumerable)task.GetType().GetProperty("Result")!.GetValue(task)!;

                var properties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var nameEnProp = properties.FirstOrDefault(p => p.Name.EndsWith("NameEn", StringComparison.OrdinalIgnoreCase));
                var nameArProp = properties.FirstOrDefault(p => p.Name.EndsWith("NameAr", StringComparison.OrdinalIgnoreCase));

                var blockingRecords = new List<object>();
                foreach (var record in records)
                {
                    var idValue = record.GetType().GetProperty(pkPropertyName)?.GetValue(record);
                    var nameEnValue = nameEnProp?.GetValue(record)?.ToString();
                    var nameArValue = nameArProp?.GetValue(record)?.ToString();

                    if (idValue != null)
                    {
                        blockingRecords.Add(new
                        {
                            id = idValue,
                            name_en = nameEnValue ?? "N/A",
                            name_ar = nameArValue ?? "N/A"
                        });
                    }
                }

                if (blockingRecords.Any())
                {
                    allConflicts.Add(new
                    {
                        tableName = tableName,
                        foreignKeyColumn = fkPropertyName,
                        blockingRecords = blockingRecords
                    });
                }
            }

            return new
            {
                message = "This record cannot be deleted because dependent records exist in other tables.",
                attemptedDeleteId = entityIdValue,
                conflicts = allConflicts.Any() ? (object)allConflicts : "Unable to determine dynamic conflicts, manual check required."
            };
        }
    }
}