using NUnit.Framework;
using UnityEngine;
using Attacks;

public class AttackSpawnTests
{
    private GameObject go;
    private TestAttackSpawn spawn;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("SpawnGO");
        spawn = go.AddComponent<TestAttackSpawn>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Init_RaisesSpawnEvent()
    {
        // Provide dummy init/stats objects - AttackInit and AttackStats are abstract; create simple derived ones here.
        var init = new DummyInit();
        var stats = new DummyStats();

        spawn.Init(init, stats);

        Assert.IsTrue(spawn.SpawnRaised);
    }

    private class DummyInit : AttackInit
    {
        public DummyInit(LayerMask hitMask = default, Vector2 position = default, Quaternion rotation = default, Transform parent = null) : base(hitMask, position, rotation, parent)
        {
        }
    }
    private class DummyStats : AttackStats
    {
        public DummyStats(int damage = default, float size = default, float criticalChance = default, float criticalDamageMultiplier = default, float knockback = default, float hitRefreshRate = default, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHits = false) : base(damage, size, criticalChance, criticalDamageMultiplier, knockback, hitRefreshRate, destroyProjectiles, amountOfHits, projectilesCountAsHits)
        {
        }
    }
}
