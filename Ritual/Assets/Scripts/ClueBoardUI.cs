using UnityEngine;
using UnityEngine.UI;

public class ClueBoardUI : MonoBehaviour
{
    [SerializeField] private GameObject boardPanel;
    [SerializeField] private Button boardButton;

    private void Awake()
    {
        boardPanel.SetActive(false);
        boardButton.onClick.AddListener(ToggleBoard);
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
        boardPanel.SetActive(!boardPanel.activeSelf);
    }
}
