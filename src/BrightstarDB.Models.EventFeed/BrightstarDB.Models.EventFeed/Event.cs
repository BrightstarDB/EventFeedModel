using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightstarDB.Models.EventFeed
{
    public partial class Event : IEvent
    {
        public dynamic GetEventData()
        {
            throw new NotImplementedException();
        }
    }
}
