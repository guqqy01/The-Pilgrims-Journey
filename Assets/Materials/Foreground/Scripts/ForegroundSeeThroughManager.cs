using Attacks;
using Heros;
using UnityEngine;

public class ForegroundSeeThroughManager : Singleton<ForegroundSeeThroughManager>
{
    [SerializeField]
    private Material foreMaterial;
    [SerializeField]
    private Material backMaterial;
    private const string heroPositionVector2Name = "_HeroPosition";

    private void Update()
    {
        if (!HeroManager.HasInstance || HeroManager.Instance.ActiveHero == null)
            return;
        foreMaterial.SetVector(heroPositionVector2Name, (Vector2)HeroManager.Instance.ActiveHero.transform.position);
        backMaterial.SetVector(heroPositionVector2Name, (Vector2)HeroManager.Instance.ActiveHero.transform.position);
    }
}
