using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UI
{
    public class DamagePopupManager : Singleton<DamagePopupManager>
    {
        [SerializeField] private DamagePopup popupPrefab;
        [SerializeField] private Transform container; // optional, for world space popups
        [SerializeField] private int defaultCapacity = 50;

        private ObjectPool<DamagePopup> pool;

        public override void InitiateSingleton()
        {
            base.InitiateSingleton();

            pool = new ObjectPool<DamagePopup>(
                createFunc: () =>
                {
                    var popup = Instantiate(popupPrefab, container);
                    popup.gameObject.SetActive(false);
                    return popup;
                },
                OnGet,
                OnRelease,
                OnDestroyDamagePopup,
                collectionCheck: true,
                defaultCapacity: defaultCapacity
            );
        }

        private void OnGet(DamagePopup damagePopup)
        {
            damagePopup.gameObject.SetActive(true);
        }

        private void OnRelease(DamagePopup damagePopup)
        {
            damagePopup.gameObject.SetActive(false);
        }

        private void OnDestroyDamagePopup(DamagePopup damagePopup)
        {
            Destroy(damagePopup.gameObject);
        }

        public void Release(DamagePopup damagePopup)
        {
            pool.Release(damagePopup);
        }

        public void SpawnPopup(Vector3 position, int value, bool isHealthDamage, Vector3 direction)
        {
            DamagePopup popup = pool.Get();
            popup.transform.position = position;
            popup.Setup(value, isHealthDamage, direction, () => Release(popup));
        }
    }
}

