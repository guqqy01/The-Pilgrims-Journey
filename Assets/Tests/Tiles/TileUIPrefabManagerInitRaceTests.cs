using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tiles.UI;

public class TileUIPrefabManagerInitRaceTests
{
    [UnityTest]
    public IEnumerator InitiateFromRootAtAwake_GuaranteesDictionaryIsBuilt_WhenInstanceBecomesAvailable()
    {
        // Reproduces the SceneRoot pattern: root + child start inactive,
        // then get activated together in one SetActive(true) call.
        GameObject rootGO = new GameObject("Root");
        rootGO.SetActive(false);

        GameObject managerGO = new GameObject("TileUIPrefabManager");
        managerGO.transform.SetParent(rootGO.transform);
        TileUIPrefabManager manager = managerGO.AddComponent<TileUIPrefabManager>();

        SceneRoot sceneRoot = rootGO.AddComponent<SceneRoot>();

        // This single call is what used to race: Unity's own Awake() for
        // `manager` vs. SceneRoot.Awake() manually calling InitiateFromRootAtAwake().
        rootGO.SetActive(true);

        yield return null;

        Assert.IsTrue(TileUIPrefabManager.HasInstance, "Instance should be set after activation.");
        Assert.IsNotNull(manager.TileTypeAndUIPrefab,
            "Dictionary must be non-null as soon as Instance is available — " +
            "if this fails, InitiateFromRootAtAwake and Awake() have raced apart again.");

        Object.Destroy(rootGO);
    }
}