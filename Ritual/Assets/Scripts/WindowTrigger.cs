using UnityEngine;

public class WindowTrigger : MonoBehaviour
{
    public ManagerRoomDialogue conversation;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            conversation.gameObject.SetActive(true);
        }
    }
}
