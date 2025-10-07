using UnityEngine;
using DialogueSystem;
using Map;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
public class DirectionChoiceManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static DirectionChoiceManager Instance;
    public Prompt directionChoicePrompt;

    [SerializeField]
    private DialogueEventListener directionChoiceListener = new DialogueEventListener();

    private List<DialogueEventListener> promptDirectionListeners = new List<DialogueEventListener>();

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public void SetupDirectionChoicePrompt()
    {
        directionChoicePrompt.promptResponses = new List<PromptResponse>();
        promptDirectionListeners.Clear();

        foreach (Direction direction in GlobalMapGenerator.Instance.CurrentMapPoint.connectedToPoints.Keys) 
        {
            if (GlobalMapGenerator.Instance.takenPath.Contains(GlobalMapGenerator.Instance.CurrentMapPoint.connectedToPoints[direction]))
                continue;

            PromptResponse response = new PromptResponse() { response = direction.ToString()};
            directionChoicePrompt.promptResponses.Add(response);
            DialogueEventListener promptEventListener = new DialogueEventListener();

            promptEventListener.Event.AddListener(() => ChooseDirection(direction));
            promptEventListener.SetDialogueEvent(directionChoicePrompt, directionChoicePrompt.promptResponses.Count - 1, DialogueEventType.PromptResponse);
            promptEventListener.Register();
        }
    }


    private void OnEnable()
    {
        directionChoiceListener.Register();
    }

    private void OnDisable()
    {
        directionChoiceListener.Unregister();
    }


    public void ChooseDirection(Direction direction)
    {
        if (!GlobalMapGenerator.Instance.TryPickDirection(direction))
        {
            Debug.LogWarning($"Direction: {direction}, is not able to be picked");
        }
        GameManager.Instance.CompleteLevel();
        GameManager.Instance.LoadCurrentMapPoint();
    }
}
