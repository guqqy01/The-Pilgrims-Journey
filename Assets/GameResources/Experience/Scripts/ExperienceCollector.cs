using UnityEngine;
namespace LevelingSystem
{
    public class ExperienceCollector : MonoBehaviour
    {
        [SerializeField]
        private LayerMask experienceMask;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!experienceMask.ContainsLayer(other.gameObject.layer))
                return;

            if (other.TryGetComponent(out ExperienceOrb orb))
            {
                CollectExperience(orb);
            }
        }

        private void CollectExperience(ExperienceOrb orb)
        {
            if (!orb.gameObject.activeInHierarchy)
                return;

            ExperienceAndLevelingManager.Instance.AddExperience(orb.ExperienceAmount);

            orb.Release();
        }
    }

}