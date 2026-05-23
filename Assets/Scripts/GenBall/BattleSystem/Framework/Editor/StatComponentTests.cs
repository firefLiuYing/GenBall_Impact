using NUnit.Framework;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class StatComponentTests
    {
        private StatComponent _stats;

        [SetUp]
        public void SetUp()
        {
            _stats = new StatComponent();
        }

        [Test]
        public void GetOrCreate_NewStat_ReturnsStatWithGivenBaseValue()
        {
            var stat = _stats.GetOrCreate("Attack", 10f);
            Assert.That(stat.BaseValue, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(stat.FinalValue, Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void GetOrCreate_ExistingStat_ReturnsSameInstance()
        {
            var stat1 = _stats.GetOrCreate("Speed", 5f);
            var stat2 = _stats.GetOrCreate("Speed", 100f); // different base, should NOT override
            Assert.That(ReferenceEquals(stat1, stat2), Is.True);
            Assert.That(stat2.BaseValue, Is.EqualTo(5f)); // original value preserved
        }

        [Test]
        public void SetBase_StoresValue()
        {
            _stats.SetBase("MaxHealth", 100f);
            Assert.That(_stats.GetValue("MaxHealth"), Is.EqualTo(100f).Within(0.0001f));
        }

        [Test]
        public void SetBase_OverwritesExistingBase()
        {
            _stats.SetBase("MaxHealth", 100f);
            _stats.SetBase("MaxHealth", 200f);
            Assert.That(_stats.GetValue("MaxHealth"), Is.EqualTo(200f).Within(0.0001f));
        }

        [Test]
        public void GetValue_UnregisteredStat_ReturnsZero()
        {
            Assert.That(_stats.GetValue("NonExistent"), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void AddModifier_FlatAdd_CalculatesCorrectly()
        {
            _stats.SetBase("Attack", 10f);
            _stats.AddModifier("Attack", new StatModifier(ModifierType.FlatAdd, 5f));
            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(15f).Within(0.0001f));
        }

        [Test]
        public void AddModifier_PercentAdd_CalculatesCorrectly()
        {
            _stats.SetBase("Attack", 100f);
            _stats.AddModifier("Attack", new StatModifier(ModifierType.PercentAdd, 0.2f));
            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(120f).Within(0.0001f));
        }

        [Test]
        public void AddModifier_Multiply_CalculatesCorrectly()
        {
            _stats.SetBase("Attack", 100f);
            _stats.AddModifier("Attack", new StatModifier(ModifierType.Multiply, 1.5f));
            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(150f).Within(0.0001f));
        }

        [Test]
        public void RemoveModifier_RevertsCalculation()
        {
            _stats.SetBase("Attack", 100f);
            var modifier = new StatModifier(ModifierType.FlatAdd, 30f);
            _stats.AddModifier("Attack", modifier);
            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(130f).Within(0.0001f));

            _stats.RemoveModifier("Attack", modifier);
            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(100f).Within(0.0001f));
        }

        [Test]
        public void RemoveModifier_StatDoesNotExist_NoException()
        {
            var modifier = new StatModifier(ModifierType.FlatAdd, 10f);
            Assert.DoesNotThrow(() => _stats.RemoveModifier("NonExistent", modifier));
        }

        [Test]
        public void HasStat_Existing_ReturnsTrue()
        {
            _stats.SetBase("Defense", 5f);
            Assert.That(_stats.HasStat("Defense"), Is.True);
        }

        [Test]
        public void HasStat_NonExisting_ReturnsFalse()
        {
            Assert.That(_stats.HasStat("Magic"), Is.False);
        }

        [Test]
        public void TryGet_Existing_ReturnsTrueAndStat()
        {
            _stats.SetBase("Speed", 8f);
            Assert.That(_stats.TryGet("Speed", out var stat), Is.True);
            Assert.That(stat, Is.Not.Null);
            Assert.That(stat.BaseValue, Is.EqualTo(8f).Within(0.0001f));
        }

        [Test]
        public void TryGet_NonExisting_ReturnsFalse()
        {
            Assert.That(_stats.TryGet("Luck", out var stat), Is.False);
            Assert.That(stat, Is.Null);
        }

        [Test]
        public void MultipleStats_Independent()
        {
            _stats.SetBase("Attack", 10f);
            _stats.SetBase("Defense", 5f);
            _stats.AddModifier("Attack", new StatModifier(ModifierType.FlatAdd, 3f));

            Assert.That(_stats.GetValue("Attack"), Is.EqualTo(13f).Within(0.0001f));
            Assert.That(_stats.GetValue("Defense"), Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void AddModifier_AutoCreatesStatWithBaseZero()
        {
            _stats.AddModifier("NewStat", new StatModifier(ModifierType.FlatAdd, 10f));
            Assert.That(_stats.GetValue("NewStat"), Is.EqualTo(10f).Within(0.0001f));
        }
    }
}
