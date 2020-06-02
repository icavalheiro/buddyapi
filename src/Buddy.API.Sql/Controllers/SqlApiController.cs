using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using Buddy.API.Models;
using Buddy.API.Controllers;

namespace Buddy.API.Sql.Controllers
{
    /// <summary>
    /// API controller that depends on a SQL database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class SqlApiController<T> : ApiController<T> where T : Entity, new()
    {
        /// <summary>
        /// A database context to be used
        /// </summary>
        private DbContext _context;

        /// <summary>
        /// The DB set that corresponds to that type of entity
        /// </summary>
        private DbSet<T> _dbSet;

        public SqlApiController(DbContext db)
        {
            _context = db;
            _dbSet = _context.Set<T>();
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
        protected override T CreateEntity(T entity)
        {
            if (entity.Id != Guid.Empty &&
                _dbSet
                    .Where(x => x.Id == entity.Id)
                    .Take(1)
                    .Count() > 0)
                return null;

            _dbSet.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Deletes an entity with the given ID from the storage.
        /// Returns false if no entity matches the given ID
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>Ture if it sucessfully found and deleted the entity</returns>
        protected override bool DeleteEntity(Guid id)
        {
            var current = GetEntity(id).FirstOrDefault();
            if (current != null)
            {
                current.DeletionDate = DateTime.Now;
                return UpdateEntity(id, current) != null;
            }

            return false;
        }

        /// <summary>
        /// Returns a query with all the entities in the database.
        /// You should override this if you need to use any special ".includes".
        /// </summary>
        /// <returns>A Queryable with all the entities.</returns>
        protected override IQueryable<T> GetAllEntities()
        {
            return _dbSet
                    .Where(x => x.DeletionDate == null);
        }

        /// <summary>
        /// Returns a query with only 1 entity that matches the given ID.
        /// It uses the GetAllEntities() method under the hood, so if you
        /// need to add any special "includes()" you could do only in the
        /// GetAllEntities method.
        /// </summary>
        /// <param name="id">Id if the Entity</param>
        /// <returns>The IQueryable with the entity entry</returns>
        protected override IQueryable<T> GetEntity(Guid id)
        {
            return GetAllEntities()
                    .Where(x => x.Id == id)
                    .Take(1);
        }

        /// <summary>
        /// Updates an entity witht he given ID with the given entity.
        /// The entity must exists, it does not create new entities.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="entity">Entity to be used as the update agent</param>
        /// <returns>The updated entity otherwise null</returns>
        protected override T UpdateEntity(Guid id, T entity)
        {
            var current = GetEntity(id).FirstOrDefault();
            if (current != null)
            {
                entity.Id = current.Id;
                current = entity;
                current.LastUpdateDate = DateTime.Now;
                _dbSet.Update(current);
                _context.SaveChanges();
                return current;
            }

            return null;
        }
    }
}
