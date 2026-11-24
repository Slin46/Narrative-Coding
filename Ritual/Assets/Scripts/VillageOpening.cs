using UnityEngine;
using Yarn.Unity;

public class VillageOpening : MonoBehaviour
{
    public static bool hasVisited;
    public GameObject openingPanel;
    public GameObject openingBackground;
    public GameObject controlsPrompt;
    public DialogueRunner dialogueRunner;
    public PlayerMovement playerMovement;
    public GuestHouseEnter guestHouseDoor;
    public GuestHouseEnter radioRoomDoor;
    public GuestHouseEnter farmDoor;
    public SceneManagerScript sceneManager;

    void Awake()
    {
        if (controlsPrompt != null)
        {
            controlsPrompt.SetActive(false);
        }

        guestHouseDoor.canEnter = false;
        radioRoomDoor.canEnter = false;
        sceneManager.exitDoor = guestHouseDoor;
        openingBackground.SetActive(!hasVisited && farmDoor == null);

        if (farmDoor != null)
        {
            farmDoor.canEnter = false;
            dialogueRunner.startNode = "Act2Village";
            sceneManager.exitDoor = farmDoor;
            playerMovement.transform.SetPositionAndRotation(
                radioRoomDoor.spawnPoint.position, radioRoomDoor.spawnPoint.rotation);
        }
        else if (hasVisited)
        {
            dialogueRunner.startNode = "VillageReturn";
            sceneManager.exitDoor = radioRoomDoor;
            playerMovement.transform.SetPositionAndRotation(
                guestHouseDoor.spawnPoint.position, guestHouseDoor.spawnPoint.rotation);
        }

        hasVisited = true;
        dialogueRunner.autoStart = true;
        openingPanel.SetActive(true);
        playerMovement.canMove = false;
        dialogueRunner.onDialogueComplete.AddListener(ClosePanel);
    }

    void ClosePanel()
    {
        openingPanel.SetActive(false);
        if (controlsPrompt != null)
        {
            controlsPrompt.SetActive(sceneManager.exitDoor == guestHouseDoor);
        }
        playerMovement.canMove = true;
        sceneManager.exitDoor.CanEnter();
        dialogueRunner.onDialogueComplete.RemoveListener(ClosePanel);
    }
}
