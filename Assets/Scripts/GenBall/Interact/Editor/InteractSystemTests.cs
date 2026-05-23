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
            public bool WasInteracted { get; private set; }
            public void Interact() { WasInteracted = true; }
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
            // Arrange
            var a = new StubInteractable();
            var b = new StubInteractable();

            // Act
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);

            // Assert
            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddInteractable_DuplicateIgnored()
        {
            // Arrange
            var a = new StubInteractable();

            // Act
            _interact.AddInteractable(a);
            _interact.AddInteractable(a);

            // Assert
            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveInteractable_DecreasesCount()
        {
            // Arrange
            var a = new StubInteractable();
            _interact.AddInteractable(a);

            // Act
            _interact.RemoveInteractable(a);

            // Assert
            Assert.That(_interact.Interactables.Value.Count, Is.EqualTo(0));
        }

        [Test]
        public void NextSelection_WrapsAround()
        {
            // Arrange
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            // Assert initial
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));

            // Act & Assert: 0 -> 1
            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));

            // Act & Assert: 1 -> 2
            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(2));

            // Act & Assert: 2 -> 0 (wrap)
            _interact.NextSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));
        }

        [Test]
        public void LastSelection_WrapsAround()
        {
            // Arrange
            var a = new StubInteractable();
            var b = new StubInteractable();
            var c = new StubInteractable();
            _interact.AddInteractable(a);
            _interact.AddInteractable(b);
            _interact.AddInteractable(c);

            // Assert initial
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(0));

            // Act & Assert: 0 -> 2 (wrap backward)
            _interact.LastSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(2));

            // Act & Assert: 2 -> 1
            _interact.LastSelection();
            Assert.That(_interact.CurrentSelectionIndex.Value, Is.EqualTo(1));
        }

        [Test]
        public void TriggerInteractable_CallsInteract()
        {
            // Arrange
            var stub = new StubInteractable();
            _interact.AddInteractable(stub);

            // Act
            _interact.TriggerInteractable();

            // Assert
            Assert.That(stub.WasInteracted, Is.True);
        }

        [Test]
        public void NextSelection_EmptyList_NoError()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _interact.NextSelection());
        }

        [Test]
        public void TriggerInteractable_EmptyList_NoError()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _interact.TriggerInteractable());
        }
    }
}
