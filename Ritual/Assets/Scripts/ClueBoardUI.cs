using UnityEngine;
using UnityEngine.UI;

public class ClueBoardUI : MonoBehaviour
{
    [SerializeField] private GameObject boardPanel;
    [SerializeField] private Button boardButton;

    private bool audioWasPaused;

    public bool IsOpen => boardPanel.activeSelf;

    private void Awake()
    {
        boardPanel.SetActive(false);
        boardButton.onClick.AddListener(ToggleBoard);
        boardButton.transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBoard();
        }
    }

    private void ToggleBoard()
    {
        if (boardPanel.activeSelf)
        {
            CloseBoard();
        }
        else
        {
            OpenBoard();
        }
    }

    private void OpenBoard()
    {
        PauseMenuController pauseMenu = FindFirstObjectByType<PauseMenuController>();
        if (pauseMenu != null && pauseMenu.IsOpen)
        {
            return;
        }

        audioWasPaused = AudioListener.pause;
        AudioListener.pause = true;
        Time.timeScale = 0f;
        boardPanel.SetActive(true);
        boardButton.transform.SetAsLastSibling();
    }

    private void CloseBoard()
    {
        boardPanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = audioWasPaused;
    }
}
