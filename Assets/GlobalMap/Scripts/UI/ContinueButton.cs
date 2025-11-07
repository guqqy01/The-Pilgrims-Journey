using UnityEngine;
namespace GlobalMap.UI
{
    public class ContinueButton : MonoBehaviour
    {
        public void OnButtonClick()
        {
            GameManager.Instance.LoadCurrentMapPoint();
        }
    }
}

