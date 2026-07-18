using System;
using SiPVLib.Config.Configs;
using SiPVLib.Config.GameConditions;
using SiPVLib.Quest.Enums;
using SiPVLib.Utilities;
using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Static configuration for a quest.
    /// Inherits from GameConfig for config management integration.
    /// Stores quest metadata, objectives, and rewards.
    /// </summary>
    public class ConfigQuest : GameConfig
    {
        [SerializeField]
        [Tooltip("Human-readable title of the quest.")]
        protected string _title;

        [SerializeField]
        [Tooltip("Detailed description of the quest.")]
        [TextArea(3, 5)]
        protected string _description;

        [SerializeField]
        [Tooltip("Conditions that must be met for this quest to be unlocked. All conditions must be satisfied.")]
        [SubAsset]
        protected GameConditionGroup _unlockedConditions;

        [SerializeField]
        [Tooltip("How objectives are executed: Sequential (one after another) or Parallel (all at once).")]
        protected ObjectiveExecutionMode _executionMode = ObjectiveExecutionMode.Sequential;

        [SerializeField]
        [Tooltip("List of objectives that comprise this quest.")]
        protected QuestObjective[] _objectives = Array.Empty<QuestObjective>();

        [SerializeField]
        [Tooltip("Rewards granted when the quest is completed.")]
        protected QuestReward _reward;

        [SerializeField]
        [Tooltip("Can this quest be repeated after completion?")]
        protected bool _isRepeatable;

        #region Properties

        public string Title => _title;
        public string Description => _description;
        public GameConditionGroup UnlockedConditions => _unlockedConditions;
        public ObjectiveExecutionMode ExecutionMode => _executionMode;
        public QuestObjective[] Objectives => _objectives;
        public QuestReward Reward => _reward;
        public bool IsRepeatable => _isRepeatable;

        #endregion

        public override string ToString() => $"Quest [{Id}]: {Title}";
    }
}

