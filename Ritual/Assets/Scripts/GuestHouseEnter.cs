using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuestHouseEnter : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public float waitDuration = 1f;
    public string destinationSceneName = "GuestHouse";

    public Transform spawnPoint;

    private PlayerMovement playerMovement;

    private bool isEntering = false;
    //set can enter to false to prevent player from advcancing to the next scene prior to finishing dialogue and puzzles
    private bool canEnter = false;

    //call this function when dialogue or puzzle is finished
    public void CanEnter()
    {
        canEnter = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") &&
            Input.GetKey(KeyCode.W) &&
            !isEntering)
        {
            isEntering = true;

            StartCoroutine(EnterGuestHouse());

            //disable player movement when entering the next scene
            playerMovement = other.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.canMove = false;
            }
        }
    }

    IEnumerator EnterGuestHouse()
    {
        yield return new WaitForSeconds(waitDuration);

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
        SceneManager.LoadScene(destinationSceneName);
    }

}


