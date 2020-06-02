using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buddy.API.Services
{
    public class MongoEntityService : EntityService
    {

        private readonly IMongoDatabase _db;

        public MongoEntityService(IMongoDatabase db)
        {
            _db = db;
        }

        private IMongoCollection<T> GetCollection<T>() where T : class, IEntity => _db.GetCollection<T>(nameof(T));

        /// <summary>
        /// Creates the given entity into the storage system.
        /// Returns null if this entity already exists.
        /// </summary>
        /// <param name="entity">Entity to be created</param>
        /// <returns>Created entity</returns>
        public override T Create<T>(T entity)
        {
            if (entity.Id != Guid.Empty && Get<T>(entity.Id).FirstOrDefault() != null)
            {
                return null;
            }

            GetCollection<T>().InsertOne(entity);
            return entity;
        }

        /// <summary>
        /// Deletes an entity with the given ID from the storage.
        /// Returns false if no entity matches the given ID
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>True if it sucessfully found and deleted the entity</returns>
        public override bool Delete<T>(Guid id)
        {
            var entity = Get<T>(id).FirstOrDefault();
            if (entity == null)
            {
                return false;
            }

            entity.DeletionDate = DateTime.Now;
            return Update(id, entity) != null;
        }

        /// <summary>
        /// Returns a query with only 1 entity that matches the given ID.
        /// It uses the GetAllEntities() method under the hood.
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
        /// </summary>
        /// <returns>A Queryable with all the entities.</returns>
        public override IQueryable<T> GetAll<T>()
        {
            return GetCollection<T>()
                    .AsQueryable()
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
            entity.LastUpdateDate = DateTime.Now;
            var replaceOperation = GetCollection<T>().ReplaceOne(x => x.Id == id, entity);
            if (replaceOperation.MatchedCount > 0)
            {
                return entity;
            }

            return null;
        }
    }
}
