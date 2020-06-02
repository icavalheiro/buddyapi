using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Linq;
using Buddy.API.Enumerators;

namespace Buddy.API.Controllers
{
    /// <summary>
    /// API Controller that handles all basic CRUD operations.
    /// You can override any of the crud endpoints or you can 
    /// override the methods that the controller uses to interact
    /// with the database.
    /// You can rely on the base behaviour for most of the things.
    /// You should overrite only the methods that have a custom 
    /// functionality that differs from the default.
    /// </summary>
    /// <typeparam name="T">Model type that this controller will handle</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController<T> : ControllerBase where T : IEntity, new()
    {
        private static JsonResult _modelResult;
        private readonly IEntityService _entityService;

        public ApiController(IEntityService entityService)
        {
            _entityService = entityService;

            //Loads the model cache if it's not cached yet
            if(_modelResult == null)
            {
                _modelResult = new JsonResult(_entityService.GenerateModel<T>());
            }
        }

        /// <summary>
        /// Default route: GET api/[controller]/model?example=false
        /// 
        /// Returns the model json of the model type that this
        /// controller handles. Usefull if you need to handle
        /// validations in the front-end, also exports the
        /// "RenderAs" attributes that allow you to render custom
        /// elements/behaviours/inputs for the properties of the
        /// model
        /// 
        /// Regarding the example flag: if it's set it will return a
        /// "new" isntance of this controllers model: "new T()"
        /// So if you want it to return the model with sample values
        /// you can do that directly in your model constructor declaration.
        /// </summary>
        /// <param name="example">Should we return a simple example of the object of the full model?</param>
        /// <returns>JSON of the model type or an example of the object if example == true</returns>
        [HttpGet("model")]
        [Produces("application/json", Type = typeof(OkResult))]
        public virtual JsonResult Model([FromQuery]bool? example = false)
        {
            example = example ?? false;
            if ((bool)example)
            {
                return new JsonResult(new T());
            }

            return _modelResult;
        }

        /// <summary>
        /// Returns an IQueryable with all entities from the entity service.
        /// </summary>
        /// <returns>IQueryable instance</returns>
        protected virtual IQueryable<T> GetAllEntities()
        {
            return _entityService.GetAll<T>();
        }

        /// <summary>
        /// Returns an IQueryable with one element with the given ID form the 
        /// entity service.
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>IQueryable instance</returns>
        protected virtual IQueryable<T> GetEntity(Guid id)
        {
            return _entityService.Get<T>(id);
        }

        /// <summary>
        /// Creates the given entity witht the enitity service.
        /// Should return null if process failed somehow.
        /// </summary>
        /// <param name="entity">Entity to be created</param>
        /// <returns>Created entity</returns>
        protected virtual T CreateEntity(T entity)
        {
            return _entityService.Create<T>(entity);
        }

        /// <summary>
        /// Updates an entity with the given ID.
        /// /// Should return false if no entity matches the given ID.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="entity">Entity to be used as the update agent</param>
        /// <returns>The updated entity otherwise null</returns>
        protected virtual T UpdateEntity(Guid id, T entity)
        {
            return _entityService.Update<T>(id, entity);
        }

        /// <summary>
        /// Deletes an entity with the given ID from the entity service.
        /// Should return false if no entity matches the given ID.
        /// </summary>
        /// <param name="id">Id of the entity to be deleted</param>
        /// <returns>Ture if it sucessfully found and deleted the entity</returns>
        protected virtual bool DeleteEntity(Guid id)
        {
            return _entityService.Delete<T>(id);
        }

        /// <summary>
        /// Route: GET api/[controller]
        /// 
        /// Lists all the entities that this controller handles.
        /// You can paginate it by sending "?page=1" (it starts at 0)
        /// You can also set the number of itens each page would have with ?pageSize=20
        /// If "?page=-1" the itens won't be paginated
        /// </summary>
        /// <returns>All with the entities that this controller handles or a page if "?page" is defined</returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(OkResult))]
        public virtual ActionResult Get([FromQuery] int page = -1, [FromQuery] int pageSize = 20)
        {
            if (!ValidateAuthFor(CrudType.LIST))
                return Unauthorized();

            var entities = GetAllEntities();
            
            if(page > -1)
            {
                var total = entities.Count();

                //User wants to paginate
                entities = entities.Skip(page * pageSize).Take(pageSize);

                return new JsonResult(new 
                {
                    items = entities.ToArray(),
                    total,
                    page,
                    pageSize
                });
            }

            return new JsonResult(entities.ToArray());

        }

        /// <summary>
        /// Route: GET api/[controller]/{id}
        /// 
        /// Get an entity by it's id.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns>The entity, otherwise 404</returns>
        [HttpGet("{id}")]
        [Produces("application/json", Type = typeof(OkResult))]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        public virtual ActionResult Get([FromRoute] Guid id)
        {
            if (!ValidateAuthFor(CrudType.GET))
                return Unauthorized();

            var entity = GetEntity(id).FirstOrDefault();
            if(entity != null)
            {
                return new JsonResult(entity);
            }

            return NotFound();
        }

        /// <summary>
        /// Route: POST api/[controller]
        /// 
        /// Stores the given entity in the provider.
        /// The entity must be valid in order to access this callback.
        /// </summary>
        /// <param name="entity">Entity to be stored</param>
        /// <returns>The created entity</returns>
        [HttpPost]
        [Produces("application/json", Type = typeof(CreatedResult))]
        [ProducesErrorResponseType(typeof(ConflictResult))]
        public virtual ActionResult Post([FromBody] T entity)
        {
            if (!ValidateAuthFor(CrudType.ADD))
                return Unauthorized();

            if(entity.Id != Guid.Empty && GetEntity(entity.Id).FirstOrDefault() != null)
            {
                return Conflict();
            }

            var savedEntity = CreateEntity(entity);
            Response.StatusCode = (int)HttpStatusCode.Created;
            return new JsonResult(savedEntity);
        }

        /// <summary>
        /// Route: PUT api/[controller]/{id}
        /// 
        /// Updated an existing entity with the given entity
        /// </summary>
        /// <param name="id">Entity to replace</param>
        /// <param name="entity">The new entity</param>
        /// <returns>The new entity</returns>
        [HttpPut("{id}")]
        [Produces("application/json", Type = typeof(OkResult))]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        public virtual ActionResult Put([FromRoute] Guid id, [FromBody] T entity)
        {
            if (!ValidateAuthFor(CrudType.EDIT))
                return Unauthorized();

            if (id == Guid.Empty)
            {
                id = entity.Id;
            }

            var replacedEntity = UpdateEntity(id, entity);
            if(replacedEntity == null)
            {
                return NotFound();
            }

            return new JsonResult(replacedEntity);
        }

        /// <summary>
        /// Route: DELETE api/[controller]/{id}
        /// 
        /// Deletes the entity with the given ID.
        /// </summary>
        /// <param name="id">ID of the entity to be deleted</param>
        [HttpDelete("{id}")]
        [Produces(typeof(NoContentResult))]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        public virtual ActionResult Delete([FromRoute] Guid id)
        {
            if (!ValidateAuthFor(CrudType.DELETE))
                return Unauthorized();

            if(DeleteEntity(id))
            {
                return NoContent();
            }

            return NotFound();
        }

        /// <summary>
        /// Validates if the current user can do the given action.
        /// Override this if you want to controll the actions a given user can do
        /// in this controller, you can switch the CrudType and use the "Can(string)"
        /// method to validate if the current logged user has a given permission.
        /// </summary>
        /// <param name="action">The CRUD action being performed</param>
        /// <returns>If the current user is authorized to perform the action</returns>
        protected virtual bool ValidateAuthFor(CrudType action)
        {
            //we dont mind the action here,
            //lets consider all the same
            //and any user can access
            return true;
        }
    }

}
