using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using BrightstarDB.Client;
using BrightstarDB.EntityFramework;
using BrightstarDB.Dynamic;

namespace BrightstarDB.Models.EventFeed
{
    /// <summary>
    /// The event feed service provides the functional 
    /// capabilities and uses the data model defined.
    /// </summary>
    public class EventFeedService : IEventFeedService
    {
        private readonly string _connectionString;
        private readonly string _storeName;

        public EventFeedService(string storeNane, string connectionString = "type=embedded;storesdirectory=c:\\brightstar")
        {
            _storeName = storeNane;
            if (connectionString.Contains("storename=")){
                _connectionString = connectionString;            
            } else {
                _connectionString = connectionString + ";storename=" + storeNane; 
            }
        }

        public void AssertSubscriber(string userName, IEnumerable<Uri> topicsOfInterest)
        {
            try {
                var ctx = new EventFeedContext(_connectionString);

                // check if we have one
                if (ctx.Subscribers.Where(s => s.UserName.Equals(userName)).Count() > 0)
                {
                    return;
                }

                var subscriber = ctx.Subscribers.Create();
                subscriber.UserName = userName;
                var topics = new List<ITopic>();
                foreach (var topicId in topicsOfInterest)
                {
                    var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();
                    if (topic != null)
                    {
                        topics.Add(topic);
                    }                    
                }
                subscriber.Topics = topics;

                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RegisterSubscriber", ex);
            }
        }

        /// <summary>
        /// Gets all the events on the subscriber timeline. Optional parameters are since and also how many
        /// </summary>
        /// <param name="userName">Gets timeline for user</param>
        /// <param name="since">Events since this data should be included</param>
        /// <returns>Enumeration of events</returns>
        public IEnumerable<IEvent> GetSubscriberTimeline(string userName, DateTime since)
        {
            try
            {
                var ctx = new EventFeedContext(_connectionString);
                var sub = ctx.Subscribers.Where(s => s.UserName.Equals(userName)).ToList().FirstOrDefault();
                if (sub == null) throw new Exception("Subscriber does not exist");
                return sub.Events.Where(e => e.Occurred > since).OrderBy(e => e.Occurred);
            } catch (Exception ex){
                throw new Exception("Error in GetSubscriberTimeline", ex);
            }
        }

        public IEnumerable<IEvent> GetTopicTimeline(string topicId, DateTime since)
        {
            try
            {
                var ctx = new EventFeedContext(_connectionString);
                var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();
                if (topic == null) return new List<IEvent>();
                return topic.Events.Where(e => e.Occurred > since).OrderBy(e => e.Occurred);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetTopicTimeline", ex);
            }
        }

        public void AssertTopic(Uri topicId, string label, string description)
        {
            try {
                var ctx = new EventFeedContext(_connectionString);
                var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();

                if (topic != null)
                {
                    // if the topic already exists but we dont have or have different 
                    // labels and desc then update them.
                    if (!topic.Label.Equals(label)) topic.Label = label;
                    if (!topic.Description.Equals(description)) topic.Description = description;
                    if (((BrightstarEntityObject)topic).IsModified)
                    {
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    // create a new one
                    var newTopic = new Topic {Id = topicId.ToString(), Description = description, Label = label};
                    ctx.Topics.Add(newTopic);
                    ctx.SaveChanges();
                }
            } catch (Exception ex){
                throw new Exception("Error in AssertTopic", ex);
            }
        }

        public dynamic GetEventData(IEvent feedEvent)
        {
            var dynaStore = GetDynaStore();
            return dynaStore.GetDataObject(feedEvent.Id);
        }

        /// <summary>
        /// Creates a new event and also connects it to all users that are subscribed to any of the topics it is classified by.
        /// </summary>
        /// <param name="description">Event description</param>
        /// <param name="when">Date when the event occured</param>
        /// <param name="topicIds">A list of the topic ids that classify this event</param>
        /// <param name="eventProperties">A name value collection of all event properties</param>
        /// <returns></returns>
        public void RaiseEvent(string description, DateTime when, IEnumerable<string> topicIds, Dictionary<string, object> eventProperties = null)
        {
            try {
                // create a new event
                var ctx = new EventFeedContext(_connectionString);
                var e = ctx.Events.Create();
                e.Description = description;
                e.Occurred = when;
                var topics = new List<ITopic>();
                foreach (var topicId in topicIds)
                {
                    var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();
                    if (topic != null)
                    {   
                        topics.Add(topic);
                        
                        // get all the subscribers
                        foreach (var subscriber in topic.Subscribers)
                        {
                            subscriber.Events.Add(e);
                        }
                    }                    
                }
                e.Topics = topics;

                ctx.SaveChanges();

                if (eventProperties != null)
                {
                    // use a dynamic object to store all the other properties
                    var dynaStore = GetDynaStore();
                    var dynaEvent = dynaStore.GetDataObject(e.Id);

                    foreach (var key in eventProperties.Keys)
                    {
                        dynaEvent[key] = eventProperties[key];
                    }
                    dynaStore.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RasieEvent", ex);
            }
        }

        public void RegisterInterest(string userName, string topicId)
        {
            var ctx = new EventFeedContext(_connectionString);
            var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();
            var subscriber = ctx.Subscribers.Where(s => s.UserName.Equals(userName)).ToList().FirstOrDefault();

            if (topic == null) { throw new Exception("No topic with id");}
            if (subscriber == null) throw new Exception("No subscriber with name provided");

            subscriber.Topics.Add(topic);
            ctx.SaveChanges();
        }

        public void RemoveInterest(string userName, string topicId)
        {
            var ctx = new EventFeedContext(_connectionString);
            var topic = ctx.Topics.Where(t => t.Id.Equals(topicId)).ToList().FirstOrDefault();
            var subscriber = ctx.Subscribers.Where(s => s.UserName.Equals(userName)).ToList().FirstOrDefault();

            if (topic == null) { throw new Exception("No topic with id"); }
            if (subscriber == null) throw new Exception("No subscriber with name provided");

            subscriber.Topics.Remove(topic);
            ctx.SaveChanges();
        }

        private DynamicStore GetDynaStore()
        {
            var dataObjectContext = BrightstarService.GetDataObjectContext(_connectionString);
            var dynaContext = new BrightstarDynamicContext(dataObjectContext);
            return dynaContext.OpenStore(_storeName);
        }


    }
}
