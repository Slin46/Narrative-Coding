using System;
using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public Collider2D targetCollider;
    public string[] conversationOrder;
    public DialogueRunner dialogueRunner;
    public Item_Cam_Rig cameraRig;
    public SpriteRenderer interactionGlow;
    public float glowOpacity = 0.5f;

    [Tooltip("This item's dialogue must finish before this object can be clicked.")]
    public DialogueTrigger prerequisite;

    private int currentIndex;
    private bool conversationInProgress;

    public bool HasCompleted => conversationOrder != null &&
        conversationOrder.Length > 0 && currentIndex >= conversationOrder.Length;

    public bool CanInteract => isActiveAndEnabled && !conversationInProgress &&
        !HasCompleted && (prerequisite == null || prerequisite.HasCompleted) &&
        dialogueRunner != null && !dialogueRunner.IsDialogueRunning &&
        dialogueRunner.DialogueTask.IsCompleted();

    void Update()
    {
        if (interactionGlow != null)
        {
            bool available = CanInteract && targetCollider != null && targetCollider.isActiveAndEnabled;
            Color color = interactionGlow.color;
            color.a = Mathf.MoveTowards(color.a, available ? glowOpacity : 0f, Time.unscaledDeltaTime * 3f);
            interactionGlow.color = color;
        }

        if (!CanInteract || !Input.GetMouseButtonDown(0) ||
            targetCollider == null || !targetCollider.isActiveAndEnabled)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector2 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
        if (targetCollider.OverlapPoint(mouseWorld))
        {
            TriggerNextConversation();
        }
    }

    public void TriggerNextConversation()
    {
        if (!CanInteract)
        {
            return;
        }

        if (conversationOrder == null || conversationOrder.Length == 0)
        {
            Debug.LogWarning("No conversations assigned to " + name, this);
            return;
        }

        string nodeName = conversationOrder[currentIndex];
        if (dialogueRunner.YarnProject == null ||
            string.IsNullOrWhiteSpace(nodeName) || !dialogueRunner.Dialogue.NodeExists(nodeName))
        {
            Debug.LogError("Assign a Yarn Project containing the node '" + nodeName + "'.", this);
            return;
        }

        RunConversation(nodeName).Forget();
    }

    void OnDisable()
    {
        if (interactionGlow != null)
        {
            Color color = interactionGlow.color;
            color.a = 0f;
            interactionGlow.color = color;
        }
    }

    private async YarnTask RunConversation(string nodeName)
    {
        conversationInProgress = true;
        try
        {
            if (cameraRig != null)
            {
                cameraRig.cam_move("item");
            }

            await dialogueRunner.StartDialogue(nodeName);
            await dialogueRunner.DialogueTask;

            if (this != null)
            {
                currentIndex++;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
        finally
        {
            if (cameraRig != null)
            {
                cameraRig.cam_move("return");
            }

            conversationInProgress = false;
        }
    }
}
