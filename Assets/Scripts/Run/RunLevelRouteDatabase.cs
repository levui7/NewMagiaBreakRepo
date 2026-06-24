using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunLevelRouteDatabase", menuName = "MagiaBreak/Run Level Route Database")]
public class RunLevelRouteDatabase : ScriptableObject
{
    [Serializable]
    public class LevelPool
    {
        [Header("Описание")]
        public string poolName = "Level 1";

        [Tooltip("Порядковый номер этапа забега. Например: 0 = первый боевой уровень, 1 = второй боевой уровень.")]
        public int orderIndex = 0;

        [Header("Вариации сцен")]
        [Tooltip("Названия сцен должны совпадать с тем, как они называются в Build Settings.")]
        public List<string> sceneNames = new List<string>();

        [Header("Правила")]
        [Tooltip("Если выключено, система постарается не выбрать ту же сцену, что была выбрана на предыдущем этапе.")]
        public bool allowSameAsPreviousStage = false;
    }

    [Header("Пулы уровней забега")]
    public List<LevelPool> levelPools = new List<LevelPool>();

    [Header("Финальная сцена")]
    [Tooltip("Сюда игра перейдет, когда закончились все пулы уровней. Например VictoryScreen.")]
    public string finalSceneName = "VictoryScreen";

    public List<LevelPool> GetSortedPools()
    {
        List<LevelPool> sorted = new List<LevelPool>();

        foreach (LevelPool pool in levelPools)
        {
            if (pool == null)
                continue;

            if (pool.sceneNames == null || pool.sceneNames.Count == 0)
                continue;

            sorted.Add(pool);
        }

        sorted.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));
        return sorted;
    }
}