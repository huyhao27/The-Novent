using System;
using System.Text.RegularExpressions;
using Novent.Core.Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Novent.Tests.Core
{
    public class EventBusTests
    {
        private readonly struct PingEvent
        {
            public PingEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TearDown]
        public void TearDown()
        {
            EventBusRegistry.ClearAll();
        }

        [Test]
        public void Publish_Subscribed_HandlerReceivesPayload()
        {
            var received = 0;
            EventBus<PingEvent>.Subscribe(e => received = e.Value);

            EventBus<PingEvent>.Publish(new PingEvent(7));

            Assert.That(received, Is.EqualTo(7));
        }

        [Test]
        public void Publish_AfterUnsubscribe_HandlerNotCalled()
        {
            var calls = 0;
            void Handler(PingEvent e) => calls++;
            EventBus<PingEvent>.Subscribe(Handler);
            EventBus<PingEvent>.Unsubscribe(Handler);

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Subscribe_Twice_HandlerCalledOnce()
        {
            var calls = 0;
            void Handler(PingEvent e) => calls++;
            EventBus<PingEvent>.Subscribe(Handler);
            LogAssert.Expect(LogType.Warning, new Regex("subscribed twice"));
            EventBus<PingEvent>.Subscribe(Handler);

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(EventBus<PingEvent>.SubscriberCount, Is.EqualTo(1));
        }

        [Test]
        public void Publish_HandlerThrows_OtherHandlersStillRun()
        {
            var reached = false;
            EventBus<PingEvent>.Subscribe(_ => throw new InvalidOperationException("boom"));
            EventBus<PingEvent>.Subscribe(_ => reached = true);
            LogAssert.Expect(LogType.Exception, new Regex("boom"));

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(reached, Is.True);
        }

        [Test]
        public void Publish_HandlerUnsubscribesLaterHandler_LaterHandlerSkipped()
        {
            var laterCalls = 0;
            void Later(PingEvent e) => laterCalls++;
            void Earlier(PingEvent e) => EventBus<PingEvent>.Unsubscribe(Later);
            EventBus<PingEvent>.Subscribe(Earlier);
            EventBus<PingEvent>.Subscribe(Later);

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(laterCalls, Is.EqualTo(0));
        }

        [Test]
        public void Publish_HandlerClearsBus_RemainingHandlersSkipped()
        {
            var laterCalls = 0;
            EventBus<PingEvent>.Subscribe(_ => EventBus<PingEvent>.Clear());
            EventBus<PingEvent>.Subscribe(_ => laterCalls++);

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(laterCalls, Is.EqualTo(0));
        }

        [Test]
        public void ClearAll_RemovesEveryHandler()
        {
            EventBus<PingEvent>.Subscribe(_ => { });

            EventBusRegistry.ClearAll();

            Assert.That(EventBus<PingEvent>.SubscriberCount, Is.EqualTo(0));
        }
    }
}
