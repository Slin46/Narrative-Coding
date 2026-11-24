using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the pause controls authored beneath a scene Canvas.
/// </summary>
public sealed class PauseMenuController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text saveStatusText;
    [SerializeField] private SceneManagerScript sceneManager;

    private bool isPaused;
    private bool audioWasPaused;

    public bool IsOpen => isPaused;

    private void Awake()
    {
        if (pausePanel == null || pauseButton == null || continueButton == null || saveButton == null || mainMenuButton == null || quitButton == null)
        {
            Debug.LogError("Pause menu references are not fully assigned.", this);
            enabled = false;
            return;
        }

        pausePanel.SetActive(false);
        pauseButton.onClick.AddListener(TogglePause);
        pauseButton.transform.SetAsLastSibling();
        continueButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void OnDisable()
    {
        ResumeGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;

        ClueBoardUI clueBoard = FindFirstObjectByType<ClueBoardUI>();
        if (clueBoard != null && clueBoard.IsOpen)
        {
            return;
        }

        isPaused = true;
        audioWasPaused = AudioListener.pause;
        AudioListener.pause = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pauseButton.transform.SetAsLastSibling();
        SetSaveStatus("Progress is saved to this device.");
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = audioWasPaused;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            SetSaveStatus("Could not find the player to save.");
            return;
        }

        GameSave.Save(SceneManager.GetActiveScene().name, player.transform.position);
        SetSaveStatus("Game saved.");
    }

    public void ReturnToMainMenu()
    {
        ResumeGame();
        if (sceneManager != null) sceneManager.ReturnToMainMenu();
        else SceneManager.LoadScene("StartScene");
    }

    public void QuitGame()
    {
        ResumeGame();
        Application.Quit();
    }

    private void SetSaveStatus(string message)
    {
        if (saveStatusText != null) saveStatusText.text = message;
    }
}
