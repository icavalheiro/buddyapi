using Buddy.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buddy.API.Services
{
    public abstract class EntityService : IEntityService
    {
        public virtual object GenerateModel<T>() where T : class, IEntity
        {
            return ModelHelper.GetModelAsJson<T>();
        }

        public abstract T Create<T>(T entity) where T : class, IEntity;

        public abstract bool Delete<T>(Guid id) where T : class, IEntity;

        public abstract IQueryable<T> Get<T>(Guid id) where T : class, IEntity;

        public abstract IQueryable<T> GetAll<T>() where T : class, IEntity;

        public abstract T Update<T>(Guid id, T entity) where T : class, IEntity;
    }
}
