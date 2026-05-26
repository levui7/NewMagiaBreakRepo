using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene names")]
    [SerializeField] private string lobbySceneName = "Lobby";

    public void StartSinglePlayer()
    {
        PlayerPrefs.SetInt("PlayerMode", 1);
        ClearSavedRunState();
        LoadLobby();
    }

    public void StartCoop()
    {
        PlayerPrefs.SetInt("PlayerMode", 2);
        ClearSavedRunState();
        LoadLobby();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ClearSavedRunState()
    {
        PlayerPrefs.DeleteKey("Player1_HP");
        PlayerPrefs.DeleteKey("Player2_HP");
        PlayerPrefs.Save();
    }

    private void LoadLobby()
    {
        if (string.IsNullOrWhiteSpace(lobbySceneName))
        {
            Debug.LogError("MainMenuController: lobbySceneName is empty. Set it to Lobby in Inspector.");
            return;
        }

        Debug.Log($"MainMenuController: loading scene '{lobbySceneName}'");
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }
}
