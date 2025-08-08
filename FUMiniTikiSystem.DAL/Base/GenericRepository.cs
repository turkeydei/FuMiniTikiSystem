using FUMiniTikiSystem.DAL.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL.Base
{
    public class GenericRepository<T> where T : class
    {
        protected FUMiniTikiSystemDbContext context;

        public GenericRepository(FUMiniTikiSystemDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public List<T> GetAll()
        {
            return context.Set<T>().ToList();
        }

        public IQueryable<T> GetAllQueryable()
        {
            return context.Set<T>().AsQueryable();
        }

        public void Create(T entity)
        {
            context.Set<T>().Add(entity);
            context.SaveChanges();
        }
        public void UpdateV2(T entity)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var keyProperties = entityType.FindPrimaryKey().Properties;

            var entityKeyValues = keyProperties
                .Select(p => entity.GetType().GetProperty(p.Name).GetValue(entity))
                .ToArray();

            var local = context.Set<T>().Local
                .FirstOrDefault(entry =>
                {
                    return keyProperties.Select(p => entry.GetType().GetProperty(p.Name).GetValue(entry))
                                        .SequenceEqual(entityKeyValues);
                });

            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }

            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void RemoveV2(T entity)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var keyProperties = entityType.FindPrimaryKey().Properties;

            var entityKeyValues = keyProperties
                .Select(p => entity.GetType().GetProperty(p.Name).GetValue(entity))
                .ToArray();

            var local = context.Set<T>().Local
                .FirstOrDefault(entry =>
                {
                    return keyProperties.Select(p => entry.GetType().GetProperty(p.Name).GetValue(entry))
                                        .SequenceEqual(entityKeyValues);
                });

            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }

            context.Entry(entity).State = EntityState.Deleted;
            context.SaveChanges();
        }


        public void Update(T entity)
        {
            var key = context.Model.FindEntityType(typeof(T)).FindPrimaryKey()
                .Properties.Select(x => x.Name).Single();

            var entityId = entity.GetType().GetProperty(key).GetValue(entity);

            var local = context.Set<T>().Local
                .FirstOrDefault(entry =>
                    entry.GetType().GetProperty(key).GetValue(entry).Equals(entityId));

            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }

            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void Remove(T entity)
        {
            var key = context.Model.FindEntityType(typeof(T)).FindPrimaryKey()
                .Properties.Select(x => x.Name).Single();

            var entityId = entity.GetType().GetProperty(key).GetValue(entity);

            var local = context.Set<T>().Local
                .FirstOrDefault(entry =>
                    entry.GetType().GetProperty(key).GetValue(entry).Equals(entityId));

            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }

            context.Entry(entity).State = EntityState.Deleted;
            context.SaveChanges();
        }

        public T GetById(int id)
        {
            return context.Set<T>().Find(id);
        }
    }
}
