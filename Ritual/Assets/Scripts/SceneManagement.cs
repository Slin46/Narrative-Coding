using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public GuestHouseEnter exitDoor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetProgress()
    {
        BedTrigger.canSleep = false;
        VillageOpening.hasVisited = false;
    }

    void Awake()
    {
        if (gameObject.scene.name == "Office" || gameObject.scene.name == "StartScene")
        {
            ResetProgress();
        }
    }

    public void NewGame()
    {
        ResetProgress();
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
        if (exitDoor != null)
        {
            if (!exitDoor.canEnter) return;
            SceneManager.LoadScene(exitDoor.destinationSceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void ReturnToGuestRoom()
    {
        BedTrigger.canSleep = true;
        SceneManager.LoadScene("GuestRoom");
    }

    public void ReturnToMainMenu()
    {
        //press return to main menu button will load main menu scene and pause audio and set time scale to 1
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(0);
    }
}
