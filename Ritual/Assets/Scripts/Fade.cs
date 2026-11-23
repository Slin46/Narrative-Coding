using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    //how long it fades
    public float fadeDuration = 1f;
    //how long it pauses
    public float displayDuration = 3f;
    public SceneManagerScript sceneManager;

    private TextMeshProUGUI warningText;

    void Start()
    {
        warningText = GetComponent<TextMeshProUGUI>();

        if (warningText == null)
        {
            Debug.LogError("No TextMeshProUGUI component found on this GameObject");
            return;
        }

        //mkae it invisible first
        Color color = warningText.color;
        color.a = 0f;
        warningText.color = color;

        StartCoroutine(FadeWarning());
    }

    IEnumerator FadeWarning()
    {
        //fade in
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            Color color = warningText.color;
            color.a = Mathf.Clamp01(timer / fadeDuration);
            warningText.color = color;

            yield return null;
        }

        //make it visible
        Color finalColor = warningText.color;
        finalColor.a = 1f;
        warningText.color = finalColor;

        //wait 3 seconds
        yield return new WaitForSecondsRealtime(displayDuration);

        //fade out
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            Color color = warningText.color;
            color.a = 1f - Mathf.Clamp01(timer / fadeDuration);
            warningText.color = color;

            yield return null;
        }

        //completely transparent again
        Color endColor = warningText.color;
        endColor.a = 0f;
        warningText.color = endColor;

        //load next scene in the build (office)
        sceneManager.NextScene();
    }
}