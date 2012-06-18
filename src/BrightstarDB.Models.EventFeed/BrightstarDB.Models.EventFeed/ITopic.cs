using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Models.EventFeed
{
    [Entity]
    public interface ITopic
    {
        /// <summary>
        /// Globally Unique Id for this topic
        /// </summary>
        string Id { get; }

        // Topic Label
        string Label { get; set; }

        // Topic Description
        string Description { get; set; }

        /// <summary>
        /// The collection of events that are clasified against this topic
        /// </summary>
        [InverseProperty("Topics")]
        ICollection<IEvent> Events { get; }
    }
}
