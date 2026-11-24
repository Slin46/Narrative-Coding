using Unity.Cinemachine;
using UnityEngine;
using Yarn.Unity;

public class NPCDialogueTrigger : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string conversation;
    public Item_Cam_Rig cameraRig;
    public CinemachineCamera playerCamera;
    public CinemachineBrain cameraBrain;
    public GuestHouseEnter exitDoor;

    private bool hasStarted;

    async void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasStarted || dialogueRunner.IsDialogueRunning)
        {
            return;
        }

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        PlayerCamera playerFollow = Camera.main.GetComponent<PlayerCamera>();
        hasStarted = true;
        playerMovement.canMove = false;

        try
        {
            if (cameraRig != null)
            {
                playerCamera.transform.position = Camera.main.transform.position;
                playerFollow.enabled = false;
                cameraBrain.enabled = true;
                await YarnTask.WaitUntil(() => cameraBrain.ActiveVirtualCamera == playerCamera);
                cameraRig.cam_move("item");
            }

            await dialogueRunner.StartDialogue(conversation);
            await dialogueRunner.DialogueTask;
        }
        finally
        {
            if (cameraRig != null)
            {
                cameraRig.cam_move("return");
                await YarnTask.WaitUntil(() => cameraBrain == null ||
                    (cameraBrain.ActiveVirtualCamera == playerCamera && !cameraBrain.IsBlending));

                if (cameraBrain != null)
                {
                    cameraBrain.enabled = false;
                }

                if (playerFollow != null)
                {
                    playerFollow.enabled = true;
                }
            }

            if (playerMovement != null)
            {
                playerMovement.canMove = true;
            }
        }

        if (exitDoor != null)
        {
            exitDoor.CanEnter();
        }
    }
}
