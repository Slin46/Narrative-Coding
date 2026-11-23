using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void NewGame()
    {
        //press new game button will load build index 1
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene("ContentWarning");
    }

    public void QuitGame()
    {
        //press quit button will close game 
        Application.Quit();
        Debug.Log("Application Closing...");
    }
    public void NextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void ReturnToMainMenu()
    {
        //press return to main menu button will load main menu scene and pause audio and set time scale to 1
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(0);
    }
}
