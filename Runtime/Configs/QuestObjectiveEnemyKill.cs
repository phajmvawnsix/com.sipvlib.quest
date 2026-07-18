using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: Kill X enemies (or reach a specific count of enemy-killed events).
    /// Example: "Defeat 5 enemies"
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveEnemyKill : QuestObjective
    {
        [SerializeField]
        [Tooltip("Optional: If set, only count kills of this enemy type. Leave empty to count all kills.")]
        protected string _enemyTypeId;

        public string EnemyTypeId => _enemyTypeId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.EnemyKillObjectiveInstance(this);
        }

        public override string ToString()
        {
            var typeInfo = string.IsNullOrEmpty(_enemyTypeId) ? "(any)" : _enemyTypeId;
            return $"{base.ToString()} | Enemy Type: {typeInfo}";
        }
    }
}

