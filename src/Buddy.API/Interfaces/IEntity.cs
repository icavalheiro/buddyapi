using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.API
{
    public interface IEntity
    {
        /// <summary>
        /// Id used in the database
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Date that the entity was created
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Date that the entity was updated
        /// </summary>
        DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// Date that this entity was deleted, if null
        /// represents that this entity was never deleted
        /// </summary>
        DateTime? DeletionDate { get; set; }
    }
}
