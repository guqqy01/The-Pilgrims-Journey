using NUnit.Framework;
using UnityEngine;
using Attacks;

public class AttackSpawnPoolTests
{
    [Test]
    public void Pool_GetAndRelease_WorksWithReturnCallback()
    {
        // This test verifies that AttackSpawnBase.DestroyAttack invokes the pool return callback when set.
        var go = new GameObject("pooltest");
        var spawn = go.AddComponent<TestAttackSpawn>();

        bool returned = false;
        spawn.SetReturnToPoolCallback(() => returned = true);

        spawn.DestroyAttack();

        Object.DestroyImmediate(go);

        Assert.IsTrue(returned);
    }
}
