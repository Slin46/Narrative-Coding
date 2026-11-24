using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSave
{
    private const string SceneKey = "Save.Scene";
    private const string PositionXKey = "Save.PositionX";
    private const string PositionYKey = "Save.PositionY";
    private const string PositionZKey = "Save.PositionZ";

    private static Vector3 savedPosition;
    private static bool shouldRestorePosition;

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SceneKey);
    }

    public static void Save(string sceneName, Vector3 playerPosition)
    {
        PlayerPrefs.SetString(SceneKey, sceneName);
        PlayerPrefs.SetFloat(PositionXKey, playerPosition.x);
        PlayerPrefs.SetFloat(PositionYKey, playerPosition.y);
        PlayerPrefs.SetFloat(PositionZKey, playerPosition.z);
        PlayerPrefs.Save();
    }

    public static void ContinueGame()
    {
        if (!HasSave())
        {
            return;
        }

        savedPosition = new Vector3(
            PlayerPrefs.GetFloat(PositionXKey),
            PlayerPrefs.GetFloat(PositionYKey),
            PlayerPrefs.GetFloat(PositionZKey));

        shouldRestorePosition = true;
        SceneManager.sceneLoaded += RestorePlayerPosition;
        SceneManager.LoadScene(PlayerPrefs.GetString(SceneKey));
    }

    private static void RestorePlayerPosition(Scene scene, LoadSceneMode mode)
    {
        if (!shouldRestorePosition)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = savedPosition;
        }

        shouldRestorePosition = false;
        SceneManager.sceneLoaded -= RestorePlayerPosition;
    }
}
