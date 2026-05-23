using NUnit.Framework;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class StatTests
    {
        [Test]
        public void SetBaseValue_UpdatesFinalValue()
        {
            var stat = new Stat(50f);
            Assert.That(stat.FinalValue, Is.EqualTo(50f).Within(0.0001f));

            stat.SetBaseValue(100f);
            Assert.That(stat.FinalValue, Is.EqualTo(100f).Within(0.0001f));
        }

        [Test]
        public void FlatAddModifier_AddedCorrectly()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(ModifierType.FlatAdd, 20f);

            stat.AddModifier(modifier);

            Assert.That(stat.FinalValue, Is.EqualTo(120f).Within(0.0001f));
        }

        [Test]
        public void PercentAddModifier_AddedCorrectly()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(ModifierType.PercentAdd, 0.3f);

            stat.AddModifier(modifier);

            Assert.That(stat.FinalValue, Is.EqualTo(130f).Within(0.0001f));
        }

        [Test]
        public void MultiplyModifier_AppliedCorrectly()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(ModifierType.Multiply, 1.5f);

            stat.AddModifier(modifier);

            Assert.That(stat.FinalValue, Is.EqualTo(150f).Within(0.0001f));
        }

        [Test]
        public void TierOrder_FlatThenPercentThenMultiply()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(ModifierType.FlatAdd, 20f));
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.1f));
            stat.AddModifier(new StatModifier(ModifierType.Multiply, 2f));

            // (100 + 20) * (1 + 0.1) * 2 = 120 * 1.1 * 2 = 264
            Assert.That(stat.FinalValue, Is.EqualTo(264f).Within(0.0001f));
        }

        [Test]
        public void RemoveModifier_RecalculatesCorrectly()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(ModifierType.FlatAdd, 20f);

            stat.AddModifier(modifier);
            Assert.That(stat.FinalValue, Is.EqualTo(120f).Within(0.0001f));

            stat.RemoveModifier(modifier);
            Assert.That(stat.FinalValue, Is.EqualTo(100f).Within(0.0001f));
        }

        [Test]
        public void SetBaseValue_AfterModifiers_Recalculates()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(ModifierType.FlatAdd, 10f));
            Assert.That(stat.FinalValue, Is.EqualTo(110f).Within(0.0001f));

            stat.SetBaseValue(200f);
            Assert.That(stat.FinalValue, Is.EqualTo(210f).Within(0.0001f));
        }

        [Test]
        public void MultipleModifiers_SameType_SumCorrectly()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(ModifierType.FlatAdd, 10f));
            stat.AddModifier(new StatModifier(ModifierType.FlatAdd, 10f));

            Assert.That(stat.FinalValue, Is.EqualTo(120f).Within(0.0001f));
        }

        [Test]
        public void NoModifiers_ReturnsBaseValue()
        {
            var stat = new Stat(75f);

            Assert.That(stat.FinalValue, Is.EqualTo(75f).Within(0.0001f));
            Assert.That(stat.BaseValue, Is.EqualTo(75f).Within(0.0001f));
        }
    }
}
