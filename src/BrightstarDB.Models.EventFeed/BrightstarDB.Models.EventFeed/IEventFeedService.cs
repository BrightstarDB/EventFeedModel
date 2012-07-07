using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightstarDB.Models.EventFeed
{
    public interface IEventFeedService
    {
        void AssertSubscriber(string userName, IEnumerable<Uri> topicsOfInterest);

        IEnumerable<IEvent> GetSubscriberTimeline(string userName, DateTime since);
        
        IEnumerable<IEvent> GetTopicTimeline(string topicId, DateTime since);
        
        void AssertTopic(Uri topicId, string label, string description);
        
        dynamic GetEventData(IEvent feedEvent);

        void RaiseEvent(string description, DateTime when, IEnumerable<string> topicIds,
                        Dictionary<string, object> eventProperties = null);

        void RegisterInterest(string userName, string topicId);

        void RemoveInterest(string userName, string topicId);
    }
}
