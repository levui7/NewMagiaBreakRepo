using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{
    [Header("Сцены")]
    public string lobbySceneName = "Lobby";
    public string mainMenuSceneName = "MainMenu";

    public void ReturnToLobby()
    {
        RunSaveSystem.ClearRunState();
        SceneManager.LoadScene(lobbySceneName);
    }

    public void ReturnToMainMenu()
    {
        RunSaveSystem.ClearRunState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartRun()
    {
        RunSaveSystem.ClearRunState();
        SceneManager.LoadScene(lobbySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Выход из игры. В редакторе Unity Application.Quit() не закрывает Play Mode.");
#endif
    }
}