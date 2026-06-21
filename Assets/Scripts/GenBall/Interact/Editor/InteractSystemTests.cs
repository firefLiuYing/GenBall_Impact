using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Interact.Tests
{
    [TestFixture]
    public class InteractSystemTests
    {
        private IInteractSystem _interact;

        private class StubInteractable : IInteractable
        {
            public string OperationDescription => "Test";
            public bool CanInteract { get; set; } = true;
            public bool WasInteracted { get; private set; }
            public int FocusedCount { get; private set; }
            public int UnfocusedCount { get; private set; }

            public void Interact() { WasInteracted = true; }
            public void OnFocused() { FocusedCount++; }
            public void OnUnfocused() { UnfocusedCount++; }

            public void ResetCounts()
            {
                FocusedCount = 0;
                UnfocusedCount = 0;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _interact = new InteractSystem();
            SystemRepository.Instance.RegisterSystem<IInteractSystem>(_interact);
        }

        [TearDown]
        public void TearDown()
        {
            SystemRepository.Instance.UnregisterSystem<IInteractSystem>();
        }

        [Test]
        public void AddInteractable_IncreasesCount()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();

            _interact.AddInteractable(a);
            _interact.AddInteractable(b);

            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddInteractable_DuplicateIgnored()
        {
            var a = new StubInteractable();

            _interact.AddInteractable(a);
            _interact.AddInteractable(a);

            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveInteractable_DecreasesCount()
        {
            var a = new StubInteractable();
            _interact.AddInteractable(a);

            _interact.RemoveInteractable(a);

            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveInteractable_ResetsIndexWhenOutOfRange()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            // Select index 2 (c)
            _interact.NextSelection();
            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(2));

            // Remove c — index 2 is now out of range (only 2 items left, valid indices 0,1)
            _interact.RemoveInteractable(c);

            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));
        }

        [Test]
        public void RemoveInteractable_KeepsIndexWhenStillInRange()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            // Select index 1 (b)
            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));

            // Remove c (index 2) — index 1 is still valid
            _interact.RemoveInteractable(c);

            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));
        }

        [Test]
        public void NextSelection_WrapsAround()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));

            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));

            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(2));

            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));
        }

        [Test]
        public void LastSelection_WrapsAround()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));

            _interact.LastSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(2));

            _interact.LastSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));
        }

        [Test]
        public void TriggerInteractable_CallsInteract()
        {
            var stub = new StubInteractable();
            _interact.AddInteractable(stub);

            _interact.TriggerInteractable();

            Assert.That(stub.WasInteracted, Is.True);
        }

        [Test]
        public void NextSelection_EmptyList_NoError()
        {
            Assert.DoesNotThrow(() => _interact.NextSelection());
        }

        [Test]
        public void TriggerInteractable_EmptyList_NoError()
        {
            Assert.DoesNotThrow(() => _interact.TriggerInteractable());
        }

        [Test]
        public void OnFocused_CalledWhenSelectionChanges()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);

            // First add sets selection to 0 with OnFocused called
            a.ResetCounts();
            b.ResetCounts();

            _interact.NextSelection();

            Assert.That(b.FocusedCount, Is.EqualTo(1));
            Assert.That(a.UnfocusedCount, Is.EqualTo(1));
        }

        [Test]
        public void OnUnfocused_CalledWhenSelectionChanges()
        {
            var a = new StubInteractable();
            var b = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);

            a.ResetCounts();
            b.ResetCounts();

            // Switch from a to b
            _interact.NextSelection();
            Assert.That(a.UnfocusedCount, Is.EqualTo(1));

            // Switch from b to a
            _interact.NextSelection();
            Assert.That(b.UnfocusedCount, Is.EqualTo(1));
        }
    }
}
