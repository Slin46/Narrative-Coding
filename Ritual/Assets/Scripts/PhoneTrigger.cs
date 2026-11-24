using UnityEngine;
using Yarn.Unity;
using Ritual.Tests;

public class PhoneTrigger : MonoBehaviour
{
    public RitualTestScene morsePanel;
    public GameObject interactPrompt;
    public PlayerMovement playerMovement;
    public PauseMenuController pauseMenu;
    public ClueBoardUI clueBoard;
    public DialogueRunner dialogueRunner;
    public ManagerRoomDialogue hideConversation;
    public GuestHouseEnter exitDoor;
    public string conversation = "CallersOtherVillage";

    private MorsePuzzleTest puzzle;
    private bool playerNearby;

    void Start()
    {
        exitDoor.canEnter = false;
        morsePanel.InitializeMorsePanel();
        puzzle = morsePanel.GetComponent<MorsePuzzleTest>();
        morsePanel.gameObject.SetActive(false);
        interactPrompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    void Update()
    {
        interactPrompt.SetActive(hideConversation.Finished && playerMovement.canMove && !puzzle.Solved);

        if (hideConversation.Finished && playerNearby && playerMovement.canMove && Time.timeScale > 0f &&
            !puzzle.Solved && Input.GetKeyDown(KeyCode.W))
        {
            OpenPanel();
        }

        if (morsePanel.gameObject.activeSelf && puzzle.Solved)
        {
            RunConversation().Forget();
        }
    }

    void LateUpdate()
    {
        if (morsePanel.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            morsePanel.gameObject.SetActive(false);
            playerMovement.canMove = true;
            pauseMenu.enabled = true;
            clueBoard.enabled = true;
        }
    }

    void OpenPanel()
    {
        playerMovement.canMove = false;
        pauseMenu.enabled = false;
        clueBoard.enabled = false;
        interactPrompt.SetActive(false);
        morsePanel.gameObject.SetActive(true);
    }

    async YarnTask RunConversation()
    {
        morsePanel.gameObject.SetActive(false);
        await dialogueRunner.StartDialogue(conversation);
        await dialogueRunner.DialogueTask;
        playerMovement.canMove = true;
        pauseMenu.enabled = true;
        clueBoard.enabled = true;
        exitDoor.CanEnter();
    }
}
