using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using DialogueSystem;
public class RandomEventGenerator : MonoBehaviour
{
    public static RandomEventGenerator Instance;

    [SerializeField]
    private WeightedChanceList<RandomEvent> randomEvents;

    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private DialogueManager dialogueManager;

    private void Awake()
    {
        Instance = this;
    }


    public void GenerateRandomEvent()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        RandomEvent randomEvent = randomEvents.GetRandom();

        backgroundImage.sprite = randomEvent.backgroundSprite;
        Dialogue dialogue = randomEvent.dialogue;

        dialogueManager.SetupDialogue(randomEvent.dialogue);
    }
}
