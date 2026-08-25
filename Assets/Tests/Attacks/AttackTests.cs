using NUnit.Framework;
using UnityEngine;
using Attacks;

public class AttackTests
{
    private TestAttack attack;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("AttackGO");
        attack = go.AddComponent<TestAttack>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ExternalTriggerAttack_OnlyWhenEnabledAndFlagSet()
    {
        attack.Active = true;
        attack.AttacksTriggeredExternally = false;
        attack.ExternalTriggerAttack();
        Assert.AreEqual(0, attack.ProtectedAttackCalls);

        attack.AttacksTriggeredExternally = true;
        attack.ExternalTriggerAttack();
        Assert.AreEqual(1, attack.ProtectedAttackCalls);
    }

    [Test]
    public void SetAttackLevel_ClampsLevelAndAppliesBuffs()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();
        attack.Setup(so, null, default);

        attack.SetAttackLevel(5); // above max
        Assert.AreEqual(so.MaxLevel, attack.Level);

        attack.SetAttackLevel(2);
        Assert.AreEqual(2, attack.Level);
    }
}
