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

    private bool isEntering = false;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") &&
            Input.GetKey(KeyCode.W) &&
            !isEntering)
        {
            isEntering = true;
            StartCoroutine(EnterGuestHouse());
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
