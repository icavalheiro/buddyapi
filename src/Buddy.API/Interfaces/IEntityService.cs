using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buddy.API
{
    public interface IEntityService
    {
        /// <summary>
        /// Should return all entities from the storage
        /// </summary>
        /// <returns>A Queryable with all the entities.</returns>
        IQueryable<T> GetAll<T>();

        /// <summary>
        /// Should return an IQueryable from which we would take the first
        /// entry.
        /// </summary>
        /// <param name="id">Id if the Entity</param>
        /// <returns>The IQueryable with the entity entry</returns>
        IQueryable<T> Get<T>(Guid id);

        /// <summary>
        /// Creates the given entity into the storage system.
        /// Should return null if this entity already exists.
        /// </summary>
        /// <param name="entity">Entity to be created</param>
        /// <returns>Created entity</returns>
        T Create<T>(T entity);

        /// <summary>
        /// Updates an entity witht he given ID with the given entity
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="entity">Entity to be used as the update agent</param>
        /// <returns>The updated entity otherwise null</returns>
        T Update<T>(Guid id, T entity);

        /// <summary>
        /// Deletes an entity with the given ID from the storage.
        /// Should return false if no entity matches the given ID
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>Ture if it sucessfully found and deleted the entity</returns>
        bool Delete<T>(Guid id);
    }
}
