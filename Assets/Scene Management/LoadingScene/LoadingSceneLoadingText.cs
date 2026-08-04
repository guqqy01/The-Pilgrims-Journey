using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadingSceneLoadingText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;
    [SerializeField]
    private float dotWritingSpeed = 0.5f;

    private float nextDotTime;

    private int currentNumberOfDots = 1;

    // Update is called once per frame
    void Update()
    {
        if(Time.time >= nextDotTime)
        {
            currentNumberOfDots = (currentNumberOfDots + 1) % 4;

            string loadingString = "Loading";

            for (int i = 0; i < currentNumberOfDots; i++)
            {
                loadingString += ".";
            }

            loadingText.text = loadingString;
            nextDotTime = Time.time + dotWritingSpeed;
        }
    }
}
