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
        
        foreach (Direction direction in GlobalMapGenerator.Instance.CurrentMapPoint.connectedToPoints.Keys) 
        {
            if (GlobalMapGenerator.Instance.takenPath.Contains(GlobalMapGenerator.Instance.CurrentMapPoint.connectedToPoints[direction]))
                continue;

            UnityEvent unityEvent = new UnityEvent();
            unityEvent.AddListener(() => ChooseDirection(direction));
            PromptResponse response = new PromptResponse() { response = direction.ToString()};
            directionChoicePrompt.promptResponses.Add(response);
        }
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
