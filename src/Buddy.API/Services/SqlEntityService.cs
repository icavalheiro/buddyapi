using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buddy.API.Services
{
    public class SqlEntityService : EntityService
    {
        private readonly DbContext _context;

        private DbSet<T> GetSet<T>() where T : class, IEntity => _context.Set<T>();

        public SqlEntityService(DbContext contex)
        {
            _context = contex;
        }

        /// <summary>
        /// Creates the given entity into the storage system.
        /// Returns null if this entity already exists.
        /// 
        /// You may override it to add custom references to the entity before
        /// its creation.
        /// </summary>
        /// <param name="entity">Entity to be created</param>
        /// <returns>Created entity</returns>
        public override T Create<T>(T entity)
        {
            var set = GetSet<T>();

            if (entity.Id != Guid.Empty &&
                set.Where(x => x.Id == entity.Id)
                   .Take(1)
                   .Count() > 0)
                return null;

            set.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Deletes an entity with the given ID from the storage.
        /// Returns false if no entity matches the given ID
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>Ture if it sucessfully found and deleted the entity</returns>
        public override bool Delete<T>(Guid id)
        {
            var current = Get<T>(id).FirstOrDefault();
            if (current != null)
            {
                current.DeletionDate = DateTime.Now;
                return Update<T>(id, current) != null;
            }

            return false;
        }

        /// <summary>
        /// Returns a query with only 1 entity that matches the given ID.
        /// It uses the GetAllEntities() method under the hood, so if you
        /// need to add any special "includes()" you could do only in the
        /// GetAllEntities method.
        /// </summary>
        /// <param name="id">Id if the Entity</param>
        /// <returns>The IQueryable with the entity entry</returns>
        public override IQueryable<T> Get<T>(Guid id)
        {
            return GetAll<T>()
                    .Where(x => x.Id == id)
                    .Take(1);
        }

        /// <summary>
        /// Returns a query with all the entities in the database.
        /// You should override this if you need to use any special ".includes".
        /// </summary>
        /// <returns>A Queryable with all the entities.</returns>
        public override IQueryable<T> GetAll<T>()
        {
            return GetSet<T>()
                    .Where(x => x.DeletionDate == null);
        }

        /// <summary>
        /// Updates an entity witht he given ID with the given entity.
        /// The entity must exists, it does not create new entities.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="entity">Entity to be used as the update agent</param>
        /// <returns>The updated entity otherwise null</returns>
        public override T Update<T>(Guid id, T entity)
        {
            var current = Get<T>(id).FirstOrDefault();
            if (current != null)
            {
                entity.Id = current.Id;
                current = entity;
                current.LastUpdateDate = DateTime.Now;
                GetSet<T>().Update(current);
                _context.SaveChanges();
                return current;
            }

            return null;
        }
    }
}
