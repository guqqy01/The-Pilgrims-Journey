using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using DialogueSystem;
using Unity.VisualScripting;
public class RandomEventGenerator : MonoBehaviour
{
    public static RandomEventGenerator Instance;
    [SerializeField]
    private WeightedChanceList<RandomEvent> initialRandomEvents;

    private WeightedChanceList<RandomEvent> dynamicRandomEvents = new WeightedChanceList<RandomEvent>();

    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private DialogueManager dialogueManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dynamicRandomEvents = new WeightedChanceList<RandomEvent>(initialRandomEvents);
    }

    public void GenerateRandomEvent()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        RandomEvent randomEvent = dynamicRandomEvents.GetRandom();

        if (randomEvent.isRepeatable == false)
            dynamicRandomEvents.Remove(randomEvent);

        backgroundImage.sprite = randomEvent.backgroundSprite;
        Dialogue dialogue = randomEvent.dialogue;

        dialogueManager.SetupDialogue(randomEvent.dialogue);
    }
}
