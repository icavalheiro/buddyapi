using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Buddy.API.Models;
using System.Linq;
using Buddy.API.Controllers;

namespace Buddy.API.Mongo.Controllers
{
    /// <summary>
    /// An API controller that depends on a mongo database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class MongoApiController<T> : ApiController<T> where T : Entity, new()
    {
        /// <summary>
        /// Mongo collection to use
        /// </summary>
        private readonly IMongoCollection<T> _collection;

        public MongoApiController(IMongoDatabase db)
        {
            _collection = db.GetCollection<T>(nameof(T));
        }

        /// <summary>
        /// Creates the given entity into the storage system.
        /// Returns null if this entity already exists.
        /// </summary>
        /// <param name="entity">Entity to be created</param>
        /// <returns>Created entity</returns>
        protected override T CreateEntity(T entity)
        {
            if (entity.Id != Guid.Empty && GetEntity(entity.Id).FirstOrDefault() != null)
            {
                return null;
            }

            _collection.InsertOne(entity);
            return entity;
        }

        /// <summary>
        /// Deletes an entity with the given ID from the storage.
        /// Returns false if no entity matches the given ID
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>True if it sucessfully found and deleted the entity</returns>
        protected override bool DeleteEntity(Guid id)
        {
            var entity = GetEntity(id).FirstOrDefault();
            if (entity == null)
            {
                return false;
            }

            entity.DeletionDate = DateTime.Now;
            return UpdateEntity(id, entity) != null;
        }

        /// <summary>
        /// Returns a query with all the entities in the database.
        /// </summary>
        /// <returns>A Queryable with all the entities.</returns>
        protected override IQueryable<T> GetAllEntities()
        {
            return _collection
                    .AsQueryable()
                    .Where(x => x.DeletionDate == null);
        }

        /// <summary>
        /// Returns a query with only 1 entity that matches the given ID.
        /// It uses the GetAllEntities() method under the hood.
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
            entity.LastUpdateDate = DateTime.Now;
            var replaceOperation = _collection.ReplaceOne(x => x.Id == id, entity);
            if (replaceOperation.MatchedCount > 0)
            {
                return entity;
            }

            return null;
        }
    }
}
