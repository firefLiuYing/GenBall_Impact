using NUnit.Framework;
using System.Collections.Generic;
using GenBall.BattleSystem.Buff;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class BuffContainerComponentTests
    {
        private BuffContainerComponent _container;

        [SetUp]
        public void SetUp()
        {
            _container = new BuffContainerComponent();
        }

        [Test]
        public void AddBuff_AddsToBuffs()
        {
            var buff = new TestBuff(1);
            _container.AddBuff(buff);
            Assert.That(_container.Buffs.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveBuff_RemovesFromBuffs()
        {
            var buff = new TestBuff(1);
            _container.AddBuff(buff);
            _container.RemoveBuff(buff);
            Assert.That(_container.Buffs.Count, Is.EqualTo(0));
        }

        [Test]
        public void Buffs_OrderedByPriority()
        {
            var lowPrio = new TestBuff(10);
            var highPrio = new TestBuff(1);
            _container.AddBuff(lowPrio);
            _container.AddBuff(highPrio);

            Assert.That(_container.Buffs[0].Priority, Is.EqualTo(1));
            Assert.That(_container.Buffs[1].Priority, Is.EqualTo(10));
        }

        [Test]
        public void NewContainer_HasEmptyBuffs()
        {
            Assert.That(_container.Buffs.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Minimal IBuff implementation for testing.
        /// Lower priority number = higher priority (DefaultComparerBuff behavior).
        /// </summary>
        private class TestBuff : IBuff
        {
            public int Priority { get; }
            public bool CanMultiExist => false;
            public IReadOnlyList<string> Tags { get; } = new List<string>();

            public TestBuff(int priority)
            {
                Priority = priority;
            }
        }
    }
}
