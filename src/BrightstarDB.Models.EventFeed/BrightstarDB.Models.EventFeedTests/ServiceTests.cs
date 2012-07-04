using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrightstarDB.Models.EventFeed;

namespace BrightstarDB.Models.EventFeedTests
{
    /// <summary>
    /// A collection of tests to exercise the event service provided.
    /// </summary>
    [TestClass]
    public class ServiceTests
    {
        private const string ConnectionString = "type=embedded;storesdirectory=c:\\brightstar";

        private static EventFeedContext GetContext(string storeId)
        {
            return new EventFeedContext(ConnectionString + ";storename=" + storeId); 
        }

        [TestMethod]
        public void TestCreateServiceInstance() 
        {
            var eventService = new EventFeedService(Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void TestAssertTopic()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");

            // use a raw context to check it exist
            var ctx = GetContext(storeId);
            Assert.AreEqual(1, ctx.Topics.Count());
            Assert.AreEqual(new Uri("http://www.brightstardb.com/topics/34"), ctx.Topics.ToList()[0].Id);
        }

        [TestMethod]
        public void TestAssertSubscriber()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");

            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });

            // use a raw context to check it exist
            var ctx = GetContext(storeId);
            Assert.AreEqual(1, ctx.Topics.Count());
            Assert.AreEqual(new Uri("http://www.brightstardb.com/topics/34"), ctx.Topics.ToList()[0].Id);

            Assert.AreEqual(1, ctx.Subscribers.Count());
            var sub = ctx.Subscribers.ToList()[0];

            Assert.AreEqual(1, sub.Topics.Count());
            Assert.AreEqual("http://www.brightstardb.com/topics/34", sub.Topics.ToList()[0].Id);
        }

        [TestMethod]
        public void TestRaiseEvent()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/34" });

            // use a raw context to check it exist
            var ctx = GetContext(storeId);
            Assert.AreEqual(1, ctx.Topics.Count());
            Assert.AreEqual(new Uri("http://www.brightstardb.com/topics/34"), ctx.Topics.ToList()[0].Id);

            Assert.AreEqual(1, ctx.Events.Count());
            var ev = ctx.Events.ToList()[0];

            Assert.AreEqual(1, ev.Topics.Count());
            Assert.AreEqual("http://www.brightstardb.com/topics/34", ev.Topics.ToList()[0].Id);
        }

        [TestMethod]
        public void TestSubscriberTimeline()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/34" });

            var events = eventService.GetSubscriberTimeline("domain\\bob", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());
        }

        [TestMethod]
        public void TestTopicTimeline()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/34" });

            var events = eventService.GetTopicTimeline("http://www.brightstardb.com/topics/34", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());
        }

        [TestMethod]
        public void TestRegisterInterest()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/33"), "Topic 33", "A very important topic");

            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/33" });
            
            eventService.RegisterInterest("domain\\bob", "http://www.brightstardb.com/topics/33");                        
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/33" });

            var events = eventService.GetSubscriberTimeline("domain\\bob", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());
        }

        [TestMethod]
        public void TestRemoveInterest()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/33"), "Topic 33", "A very important topic");

            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RegisterInterest("domain\\bob", "http://www.brightstardb.com/topics/33");

            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/33" });

            var events = eventService.GetSubscriberTimeline("domain\\bob", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());

            // remove interest
            eventService.RemoveInterest("domain\\bob", "http://www.brightstardb.com/topics/33");
            eventService.RaiseEvent("Bob created document Y", DateTime.UtcNow, new List<string>() { "http://www.brightstardb.com/topics/33" });
            events = eventService.GetSubscriberTimeline("domain\\bob", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());
        }

        [TestMethod]
        public void TestEventData()
        {
            var storeId = Guid.NewGuid().ToString();
            var eventService = new EventFeedService(storeId);

            // create a topic first that can be subscribed to, this topic could have been 'found' by search or be
            // a well known topic.
            eventService.AssertTopic(new Uri("http://www.brightstardb.com/topics/34"), "Topic 34", "A very important topic");
            eventService.AssertSubscriber("domain\\bob", new List<Uri>() { new Uri("http://www.brightstardb.com/topics/34") });
            eventService.RaiseEvent("Bob created document Y", 
                                    DateTime.UtcNow, 
                                    new List<string> { "http://www.brightstardb.com/topics/34" }, 
                                    new Dictionary<string, object> { { "DocumentUrl", "http://sharepoint.brightstardb.com"} });

            var events = eventService.GetTopicTimeline("http://www.brightstardb.com/topics/34", DateTime.MinValue);
            Assert.AreEqual(1, events.Count());

            // get event
            var ev = events.ToList()[0];
            var eventData = eventService.GetEventData(ev);
            Assert.AreEqual("http://sharepoint.brightstardb.com", eventData.DocumentUrl.ToList()[0]);

        }
    }
}
