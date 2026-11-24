using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Yarn.Unity;

public class ManagerRoomDialogue : MonoBehaviour
{
    public GameObject hidePrompt;
    public PlayerMovement playerMovement;
    public DialogueRunner dialogueRunner;
    public SceneManagerScript sceneManager;
    public Item_Cam_Rig cameraRig;
    public CinemachineCamera playerCamera;
    public CinemachineBrain cameraBrain;
    public float waitTime = 10f;
    public string conversation = "ManagerRoomConversation";
    public bool returnToGuestRoom = true;
    public Transform leavingNPC;
    public Transform npcExit;
    public float npcWalkSpeed = 3f;
    public bool Finished { get; private set; }

    IEnumerator Start()
    {
        hidePrompt.SetActive(false);
        yield return new WaitForSeconds(waitTime);
        hidePrompt.SetActive(true);
        yield return new WaitUntil(() => Time.timeScale > 0f && Input.GetKeyDown(KeyCode.S));
        hidePrompt.SetActive(false);
        RunConversation().Forget();
    }

    async YarnTask RunConversation()
    {
        Animator animator = playerMovement.GetComponent<Animator>();
        PlayerCamera playerFollow = Camera.main.GetComponent<PlayerCamera>();
        playerMovement.canMove = false;
        playerMovement.enabled = false;
        playerMovement.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsCrouching", true);

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

            if (leavingNPC != null && npcExit != null)
            {
                while (leavingNPC != null && Vector3.Distance(leavingNPC.position, npcExit.position) > 0.01f)
                {
                    leavingNPC.position = Vector3.MoveTowards(leavingNPC.position, npcExit.position, npcWalkSpeed * Time.deltaTime);
                    await YarnTask.Yield();
                }

                if (leavingNPC != null)
                {
                    leavingNPC.gameObject.SetActive(false);
                }
            }
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
                animator.SetBool("IsCrouching", false);
                playerMovement.canMove = true;
                playerMovement.enabled = true;
            }
        }

        Finished = true;

        if (this != null && returnToGuestRoom)
        {
            sceneManager.ReturnToGuestRoom();
        }
    }
}
