using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunLevelRouteManager : MonoBehaviour
{
    public static RunLevelRouteManager Instance { get; private set; }

    [Header("База вариаций уровней")]
    [SerializeField] private RunLevelRouteDatabase routeDatabase;

    [Header("Отладка")]
    [SerializeField] private bool logRoute = true;

    private const string HasRouteKey = "RunRoute_HasRoute";
    private const string RouteCountKey = "RunRoute_Count";
    private const string RouteSceneKeyPrefix = "RunRoute_Scene_";
    private const string CurrentIndexKey = "RunRoute_CurrentIndex";
    private const string SeedKey = "RunRoute_Seed";

    private readonly List<string> cachedRoute = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRouteFromPrefs();
    }

    public void StartNewRoute()
    {
        ClearSavedRoute();
        GenerateAndSaveRoute();
    }

    public string GetNextScene(string fallbackSceneName)
    {
        EnsureRouteExists();

        if (cachedRoute.Count == 0)
        {
            Debug.LogWarning("RunLevelRouteManager: маршрут пустой. Использую fallback: " + fallbackSceneName);
            return fallbackSceneName;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = cachedRoute.IndexOf(currentSceneName);

        if (currentIndex < 0)
            currentIndex = PlayerPrefs.GetInt(CurrentIndexKey, -1);

        int nextIndex = currentIndex + 1;

        if (nextIndex < 0)
            nextIndex = 0;

        if (nextIndex >= cachedRoute.Count)
        {
            string finalSceneName = GetFinalSceneName(fallbackSceneName);
            PlayerPrefs.SetInt(CurrentIndexKey, cachedRoute.Count);
            PlayerPrefs.Save();
            return finalSceneName;
        }

        string nextScene = cachedRoute[nextIndex];
        PlayerPrefs.SetInt(CurrentIndexKey, nextIndex);
        PlayerPrefs.Save();

        if (logRoute)
            Debug.Log($"RunLevelRouteManager: переход {currentSceneName} -> {nextScene} [{nextIndex + 1}/{cachedRoute.Count}]");

        return nextScene;
    }

    public string GetCurrentRouteDebugText()
    {
        EnsureRouteExists();

        if (cachedRoute.Count == 0)
            return "Маршрут забега пустой.";

        string text = "Маршрут забега:";
        for (int i = 0; i < cachedRoute.Count; i++)
            text += $"\n{i + 1}. {cachedRoute[i]}";

        return text;
    }

    public static void ClearSavedRoute()
    {
        int count = PlayerPrefs.GetInt(RouteCountKey, 0);

        for (int i = 0; i < count; i++)
            PlayerPrefs.DeleteKey(RouteSceneKeyPrefix + i);

        PlayerPrefs.DeleteKey(HasRouteKey);
        PlayerPrefs.DeleteKey(RouteCountKey);
        PlayerPrefs.DeleteKey(CurrentIndexKey);
        PlayerPrefs.DeleteKey(SeedKey);
        PlayerPrefs.Save();

        if (Instance != null)
            Instance.cachedRoute.Clear();
    }

    private void EnsureRouteExists()
    {
        LoadRouteFromPrefs();

        if (cachedRoute.Count == 0)
            GenerateAndSaveRoute();
    }

    private void GenerateAndSaveRoute()
    {
        cachedRoute.Clear();

        if (routeDatabase == null)
        {
            Debug.LogWarning("RunLevelRouteManager: routeDatabase не назначен в инспекторе.");
            return;
        }

        int seed = Random.Range(int.MinValue, int.MaxValue);
        System.Random random = new System.Random(seed);

        List<RunLevelRouteDatabase.LevelPool> pools = routeDatabase.GetSortedPools();
        string previousScene = string.Empty;

        foreach (RunLevelRouteDatabase.LevelPool pool in pools)
        {
            string selectedScene = PickSceneFromPool(pool, previousScene, random);

            if (string.IsNullOrWhiteSpace(selectedScene))
                continue;

            cachedRoute.Add(selectedScene);
            previousScene = selectedScene;
        }

        PlayerPrefs.SetInt(HasRouteKey, cachedRoute.Count > 0 ? 1 : 0);
        PlayerPrefs.SetInt(RouteCountKey, cachedRoute.Count);
        PlayerPrefs.SetInt(CurrentIndexKey, -1);
        PlayerPrefs.SetInt(SeedKey, seed);

        for (int i = 0; i < cachedRoute.Count; i++)
            PlayerPrefs.SetString(RouteSceneKeyPrefix + i, cachedRoute[i]);

        PlayerPrefs.Save();

        if (logRoute)
            Debug.Log(GetCurrentRouteDebugText());
    }

    private string PickSceneFromPool(RunLevelRouteDatabase.LevelPool pool, string previousScene, System.Random random)
    {
        if (pool == null || pool.sceneNames == null || pool.sceneNames.Count == 0)
            return string.Empty;

        List<string> candidates = new List<string>();

        foreach (string sceneName in pool.sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            if (!pool.allowSameAsPreviousStage && sceneName == previousScene)
                continue;

            candidates.Add(sceneName);
        }

        if (candidates.Count == 0)
        {
            foreach (string sceneName in pool.sceneNames)
            {
                if (!string.IsNullOrWhiteSpace(sceneName))
                    candidates.Add(sceneName);
            }
        }

        if (candidates.Count == 0)
            return string.Empty;

        int index = random.Next(0, candidates.Count);
        return candidates[index];
    }

    private void LoadRouteFromPrefs()
    {
        cachedRoute.Clear();

        if (PlayerPrefs.GetInt(HasRouteKey, 0) != 1)
            return;

        int count = PlayerPrefs.GetInt(RouteCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            string sceneName = PlayerPrefs.GetString(RouteSceneKeyPrefix + i, string.Empty);

            if (!string.IsNullOrWhiteSpace(sceneName))
                cachedRoute.Add(sceneName);
        }
    }

    private string GetFinalSceneName(string fallbackSceneName)
    {
        if (routeDatabase != null && !string.IsNullOrWhiteSpace(routeDatabase.finalSceneName))
            return routeDatabase.finalSceneName;

        return fallbackSceneName;
    }
}