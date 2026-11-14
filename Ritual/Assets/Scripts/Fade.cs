using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    //fade image to be used for the fade effect
    public Image fadeImage;
    public float fadeDuration = 1f;

    //reference to SceneManager script
    public SceneManagerScript sceneManagement;

    void Start()
    {
        StartCoroutine(Fadein());
    }

    IEnumerator Fadein()
    {
        float timer = 0f;

        //fade image set to black
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        //wait 5 seconds
        yield return new WaitForSeconds(5f);

        //call next scene from my scenemanager script
        sceneManagement.NextScene();
    }
}
