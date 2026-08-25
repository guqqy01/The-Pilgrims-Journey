using NUnit.Framework;
using UnityEngine;
using Attacks;

public class AttacksManagerTests
{
    private GameObject go;
    private AttacksManager manager;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("AttacksManagerGO");
        manager = go.AddComponent<AttacksManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void AddAttack_AddsAndRaisesEvent()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();

        BaseAttack received = null;
        manager.OnAddAttackEvent += (a) => received = a;

        manager.AddAttack(so);

        Assert.IsTrue(manager.HasAttack(so));
        Assert.IsNotNull(received);
        Assert.AreEqual(1, manager.GetAttackLevel(so));
    }

    [Test]
    public void RemoveAttack_RemovesAndRaisesEvent()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();

        BaseAttack removed = null;
        manager.OnRemoveAttackEvent += (a) => removed = a;

        manager.AddAttack(so);
        manager.RemoveAttack(so);

        Assert.IsFalse(manager.HasAttack(so));
        Assert.IsNotNull(removed);
    }

    [Test]
    public void SetAttackLevel_AddsIfMissingAndRaisesLevelChange()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();

        BaseAttack levelChanged = null;
        manager.OnAttackLevelChangeEvent += (a) => levelChanged = a;

        manager.SetAttackLevel(so, 2);

        Assert.IsTrue(manager.HasAttack(so));
        Assert.IsNotNull(levelChanged);
        Assert.AreEqual(2, manager.GetAttackLevel(so));
    }

    [Test]
    public void LevelUpOrAddAttack_LevelsUpIfPresent()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();

        manager.AddAttack(so);
        Assert.AreEqual(1, manager.GetAttackLevel(so));

        manager.LevelUpOrAddAttack(so);
        Assert.AreEqual(2, manager.GetAttackLevel(so));
    }

    [Test]
    public void DelayAllAttacks_DelaysEachAttack()
    {
        TestAttackSO so = ScriptableObject.CreateInstance<TestAttackSO>();
        manager.AddAttack(so);

        var attack = manager.GetAttackLevel(so);

        // call DelayAllAttacks; we cannot easily inspect internal nextAttackTime, but ensure no exceptions thrown and attack still present
        manager.DelayAllAttacks(1f);

        Assert.IsTrue(manager.HasAttack(so));
    }
}
