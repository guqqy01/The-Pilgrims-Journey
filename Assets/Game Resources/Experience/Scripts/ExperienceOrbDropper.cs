using UnityEngine;
namespace LevelingSystem
{
    [RequireComponent(typeof(Health))]
    public class ExperienceOrbDropper : MonoBehaviour
    {
        [SerializeField]
        private IntRangeSlider experienceWorth;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.onDie += OnDie;
        }

        private void OnDestroy()
        {
            if(health != null)
                health.onDie -= OnDie;
        }

        private void OnDie(Health _)
        {
            int min = experienceWorth.Start;
            int max = experienceWorth.End;

            int value = Random.Range(min, max + 1);

            DropExperienceOrbs(value);
        }

        private void DropExperienceOrbs(int totalExperience)
        {
            ExperienceOrbManager.Instance.SpawnOrbs(totalExperience, transform.position);
        }
    }

}