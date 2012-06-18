using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Models.EventFeed
{
    [Entity]
    public interface IEvent
    {
        string Id { get; }

        string Description { get; set; }

        DateTime Occurred { get; set; }

        ICollection<ITopic> Topics { get; set; }

        /// <summary>
        /// Returns a object that contains all the event data.
        /// The dynamic object allows us to access
        /// </summary>
        /// <returns></returns>
        dynamic GetEventData(); 
    }
}
