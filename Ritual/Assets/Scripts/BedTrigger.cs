using System.Collections;
using UnityEngine;

public class BedTrigger : MonoBehaviour
{
    public static bool canSleep;
    public GameObject nextMorningPanel;
    public PauseMenuController pauseMenu;
    public GuestHouseEnter exitDoor;
    public float panelTime = 3f;

    void Awake()
    {
        nextMorningPanel.SetActive(false);
    }

    IEnumerator Start()
    {
        if (!canSleep)
        {
            yield break;
        }

        canSleep = false;
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerMovement.canMove = false;
        pauseMenu.enabled = false;

        yield return new WaitForSeconds(1f);

        nextMorningPanel.SetActive(true);

        yield return new WaitForSeconds(panelTime);

        nextMorningPanel.SetActive(false);
        playerMovement.canMove = true;
        pauseMenu.enabled = true;
        VillageOpening.hasVisited = true;
        exitDoor.destinationSceneName = "Village";
        exitDoor.CanEnter();
    }
}
