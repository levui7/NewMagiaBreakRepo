using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string lobbySceneName = "Lobby";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject newGameModePanel;

    [Header("UI")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        ShowMainPanel();
        RefreshContinueButton();
        // Если нужно сбросить прогресс в игре
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();
    }

    public void ShowNewGamePanel()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (newGameModePanel != null)
            newGameModePanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (newGameModePanel != null)
            newGameModePanel.SetActive(false);
    }

    public void StartSinglePlayerNewGame()
    {
        StartNewGame(1);
    }

    public void StartTwoPlayerNewGame()
    {
        StartNewGame(2);
    }

    public void ContinueGame()
    {
        if (!RunSaveSystem.CanContinue())
        {
            RefreshContinueButton();
            return;
        }

        string sceneName = RunSaveSystem.GetCheckpointSceneName(lobbySceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Выход из игры. В редакторе Unity Application.Quit() не закрывает Play Mode.");
#endif
    }

    private void StartNewGame(int playerMode)
    {
        RunSaveSystem.StartNewGame(playerMode, lobbySceneName);
        SceneManager.LoadScene(lobbySceneName);
    }

    private void RefreshContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable = RunSaveSystem.CanContinue();
    }
}