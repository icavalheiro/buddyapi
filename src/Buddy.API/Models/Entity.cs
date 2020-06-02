using Buddy.API;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Buddy.API.Models
{
    /// <summary>
    /// Represents the bare minimal that the API needs the model
    /// to have in order to work properly.
    /// </summary>
    [Serializable]
    public class Entity : IEntity
    {
        /// <summary>
        /// Id used in the database
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Date that the entity was created
        /// </summary>
        public DateTime CreationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date that the entity was updated
        /// </summary>
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date that this entity was deleted, if null
        /// represents that this entity was never deleted
        /// </summary>
        public DateTime? DeletionDate { get; set; }
    }
}
