using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{
    [Header("Сцены")]
    public string lobbySceneName = "Lobby";
    public string mainMenuSceneName = "MainMenu";

    public void ReturnToLobby()
    {
        ClearSavedPlayerHealth();
        SceneManager.LoadScene(lobbySceneName);
    }

    public void ReturnToMainMenu()
    {
        ClearSavedPlayerHealth();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartRun()
    {
        ClearSavedPlayerHealth();
        SceneManager.LoadScene(lobbySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Выход из игры. В редакторе Unity Application.Quit() не закрывает Play Mode.");
#endif
    }

    private void ClearSavedPlayerHealth()
    {
        PlayerPrefs.DeleteKey("Player1_HP");
        PlayerPrefs.DeleteKey("Player2_HP");
        PlayerPrefs.Save();
    }
}