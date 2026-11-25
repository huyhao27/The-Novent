using System;
using Novent.Core;
using NUnit.Framework;

namespace Novent.Tests.Core
{
    public class ServicesTests
    {
        private sealed class Clock
        {
        }

        private sealed class DisposableService : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [TearDown]
        public void TearDown()
        {
            Services.Reset();
        }

        [Test]
        public void Get_Registered_ReturnsSameInstance()
        {
            var clock = new Clock();
            Services.Register(clock);

            Assert.That(Services.Get<Clock>(), Is.SameAs(clock));
        }

        [Test]
        public void Get_NotRegistered_ThrowsWithServiceName()
        {
            var error = Assert.Throws<InvalidOperationException>(() => Services.Get<Clock>());

            StringAssert.Contains(nameof(Clock), error.Message);
        }

        [Test]
        public void Register_Twice_Throws()
        {
            Services.Register(new Clock());

            Assert.Throws<InvalidOperationException>(() => Services.Register(new Clock()));
        }

        [Test]
        public void Reset_DisposesAndEmpties()
        {
            var service = new DisposableService();
            Services.Register(service);
            Services.MarkReady();

            Services.Reset();

            Assert.That(service.IsDisposed, Is.True);
            Assert.That(Services.TryGet<DisposableService>(out _), Is.False);
            Assert.That(Services.IsReady, Is.False);
        }
    }
}
