using UnityEngine;
using Yarn.Unity; 

public class DialogueTrigger : MonoBehaviour
{
    public Collider2D targetCollider;
    public string[] conversationOrder;   
    public DialogueRunner dialogueRunner;

    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (targetCollider.OverlapPoint(mouseWorld))
            {
                TriggerNextConversation();
            }
        }
    }

    void TriggerNextConversation()
    {
        if (conversationOrder.Length == 0)
        {
            Debug.Log("No conversations assigned!");
            return;
        }

        string nodeName = conversationOrder[currentIndex];
        Debug.Log("Starting Yarn conversation: " + nodeName);

        dialogueRunner.StartDialogue(nodeName); 

        currentIndex = Mathf.Min(currentIndex + 1, conversationOrder.Length - 1);
    }
}