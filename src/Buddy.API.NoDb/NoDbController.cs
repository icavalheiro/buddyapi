using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Buddy.API.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Newtonsoft.Json;

namespace Buddy.API.Mongo
{
    /// <summary>
    /// An API controller that does not depends on a database.
    /// It saves the entities to the File System.
    /// Meant to be used in tests only, it's prob. going to be
    /// really god damn slow, never use in production!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class NoDbController<T> : ApiController<T> where T : Entity, new()
    {
        private readonly IHostingEnvironment _env;
        private string _path { get { return _env.ContentRootPath; } }
        private string _extension { get { return $".{typeof(T).Name}.entity.json";  } }

        public NoDbController(IHostingEnvironment env)
        {
            _env = env;
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

            entity.Id = Guid.NewGuid();

            SaveEntityToDisk(entity);

            return entity;
        }

        /// <summary>
        /// Return the full path to a entity based on its ID
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns>Full path to the entity in the file system</returns>
        protected virtual string GetFileName(Guid id)
        {
            return Path.Combine(_path, id + _extension);
        }

        /// <summary>
        /// Returns the entity ion the given path
        /// </summary>
        /// <param name="path">Path to the entity</param>
        /// <returns>The entity</returns>
        protected virtual T LoadEntity(string path)
        {
            return JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(path));
        }

        /// <summary>
        /// Saved the given entity in the disk in a permanent way,
        /// </summary>
        /// <param name="entity">Entity to be saved in disk</param>
        protected virtual void SaveEntityToDisk(T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            var path = GetFileName(entity.Id);

            System.IO.File.WriteAllText(path, serialized);
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
            return Directory.GetFiles(_path, "*" + _extension)
                    .AsQueryable()
                    .Select(x => LoadEntity(x))
                    .Where(x => x.DeletionDate == null);
        }

        /// <summary>
        /// Returns a query with only 1 entity that matches the given ID.
        /// </summary>
        /// <param name="id">Id if the Entity</param>
        /// <returns>The IQueryable with the entity entry</returns>
        protected override IQueryable<T> GetEntity(Guid id)
        {
            var path = Path.Combine(_path, GetFileName(id));
            if (!System.IO.File.Exists(path))
            {
                return new T[0].AsQueryable();
            }

            return new T[] { LoadEntity(path) }
                    .AsQueryable();
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
            var currentEntity = GetEntity(id).FirstOrDefault();

            if(currentEntity != null)
            {
                entity.Id = currentEntity.Id;
                entity.LastUpdateDate = DateTime.Now;
                SaveEntityToDisk(entity);
            }

            return null;
        }
    }
}
