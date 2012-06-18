using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Models.EventFeed
{
    [Entity]
    public interface ISubscriber
    {
        string Id { get; }
        string UserName { get; set; }

        /// <summary>
        /// The set of topics that the subscribes to
        /// </summary>
        ICollection<ITopic> Topics { get; set; }

        /// <summary>
        /// These are the actual events that are added to the users timeline
        /// </summary>
        ICollection<IEvent> Events { get; set; }
    }
}
