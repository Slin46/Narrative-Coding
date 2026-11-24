using UnityEngine;
using Yarn.Unity;

public class NotebookTrigger : MonoBehaviour
{
    public GameObject notebookPanel;
    public PauseMenuController pauseMenu;
    public GuestHouseEnter exitDoor;
    public DialogueRunner dialogueRunner;

    private PlayerMovement playerMovement;
    private bool hasRead;

    void Awake()
    {
        notebookPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerMovement = other.GetComponent<PlayerMovement>();
        playerMovement.canMove = false;
        pauseMenu.enabled = false;
        notebookPanel.SetActive(true);

        if (exitDoor != null)
        {
            exitDoor.CanEnter();
        }
    }

    async void LateUpdate()
    {
        if (notebookPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            notebookPanel.SetActive(false);

            if (!hasRead && exitDoor.destinationSceneName == "ManagerRoom")
            {
                hasRead = true;
                await dialogueRunner.StartDialogue("NotebookThought");
                await dialogueRunner.DialogueTask;
            }

            playerMovement.canMove = true;
            pauseMenu.enabled = true;
        }
    }
}
