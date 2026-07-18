using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SiPVLib.Config;
using SiPVLib.Debugging;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.Quest.Enums;
using SiPVLib.Quest.Interfaces;
using SiPVLib.Quest.Runtime;
using SiPVLib.UserData;
using SiPVLib.UserData.InventoryHelper;
using SiPVLib.UserData.Utilities;
using SiPVLib.Utilities;
using UnityEngine;

namespace SiPVLib.Quest
{
    /// <summary>
    /// Central manager for the Quest system.
    /// Handles quest acceptance, state management, event dispatching, and persistence.
    /// </summary>
    public sealed class QuestManager : MonoSingleton<QuestManager>, IQuestManager
    {
        // ── Events for UI Layer ────────────────────────────────────
        /// <summary>
        /// Fired when a quest is accepted.
        /// Parameters: questId (string)
        /// </summary>
        public const string EventQuestAccepted = "Quest.Accepted";

        /// <summary>
        /// Fired when an objective's progress changes.
        /// Parameters: questId (string), objectiveId (string), currentAmount (int), requiredAmount (int)
        /// </summary>
        public const string EventObjectiveProgressChanged = "Quest.ObjectiveProgressChanged";

        /// <summary>
        /// Fired when a quest is completed (all objectives done).
        /// Parameters: questId (string)
        /// </summary>
        public const string EventQuestCompleted = "Quest.Completed";

        /// <summary>
        /// Fired when a quest reward is claimed.
        /// Parameters: questId (string)
        /// </summary>
        public const string EventQuestRewardClaimed = "Quest.RewardClaimed";

        /// <summary>
        /// Fired when a quest is abandoned.
        /// Parameters: questId (string)
        /// </summary>
        public const string EventQuestAbandoned = "Quest.Abandoned";

        // ── Fields ────────────────────────────────────────────────
        [SerializeField]
        [Tooltip("Should quest data be automatically saved whenever a quest state changes?")]
        private bool _autoSaveOnChange = true;

        private Dictionary<string, ConfigQuest> _configQuestsById;
        private Dictionary<string, QuestInstance> _activeQuestsById;

        private bool _isInitialized;

        // ── IQuestManager Properties ─────────────────────────────────

        public bool IsInitialized => _isInitialized;

        // ── Unity Lifecycle ────────────────────────────────────────

        protected override void OnSingletonInitialized()
        {
            _configQuestsById = new Dictionary<string, ConfigQuest>(StringComparer.Ordinal);
            _activeQuestsById = new Dictionary<string, QuestInstance>(StringComparer.Ordinal);
            base.OnSingletonInitialized();
        }

        // ── IQuestManager Implementation ────────────────────────────

        public async UniTask<bool> Initialize()
        {
            if (_isInitialized) return true;

            // Ensure dependencies are initialized
            if (ConfigManager.Instance == null)
            {
                CustomLog.LogError("[QuestManager] ConfigManager not found.");
                return false;
            }

            if (UserDataManager.Instance == null)
            {
                CustomLog.LogError("[QuestManager] UserDataManager not found.");
                return false;
            }

            if (!UserDataManager.Instance.IsInitialized)
            {
                var userDataOk = await UserDataManager.Instance.Init();
                if (!userDataOk)
                {
                    CustomLog.LogError("[QuestManager] Failed to initialize UserDataManager.");
                    return false;
                }
            }

            // Load all quest configs
            LoadQuestConfigs();

            // Load saved quest state from UserDataManager
            await LoadSavedQuestState();

            // Subscribe to UserData changes to re-evaluate quest unlock conditions
            SubscribeToUserDataChanges();

            // Initial check for auto-unlock conditions
            CheckAllQuestsAutoUnlockState();

            _isInitialized = true;
            CustomLog.Log("[QuestManager] Initialized successfully.");
            return true;
        }

        public QuestInstance AcceptQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                CustomLog.LogWarning("[QuestManager] Cannot accept quest with null/empty ID.");
                return null;
            }

            // Already active
            if (_activeQuestsById.TryGetValue(questId, out var existingQuest))
            {
                CustomLog.LogWarning($"[QuestManager] Quest '{questId}' is already active.");
                return existingQuest;
            }

            // Get config
            if (!_configQuestsById.TryGetValue(questId, out var config))
            {
                CustomLog.LogWarning($"[QuestManager] Quest config '{questId}' not found.");
                return null;
            }

            // Create instance and activate
            var instance = new QuestInstance(config);
            instance.Activate();

            SubscribeToQuestEvents(instance);
            instance.Subscribe();

            _activeQuestsById[questId] = instance;

            // Fire event for UI
            EventManager.Invoke(EventQuestAccepted, questId);

            CustomLog.Log($"[QuestManager] Quest accepted: {questId}");

            if (_autoSaveOnChange)
            {
                _ = SaveQuestsAsync();
            }

            return instance;
        }

        public void AbandonQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return;

            if (!_activeQuestsById.TryGetValue(questId, out var quest))
                return;

            // Removal, unsubscribe, event dispatch, and autosave all happen once via the
            // OnQuestAbandonedEvent listener registered in SubscribeToQuestEvents.
            quest.Abandon();
        }

        public QuestInstance GetActiveQuest(string questId)
        {
            _activeQuestsById.TryGetValue(questId, out var quest);
            return quest;
        }

        public void CheckQuestCompletion(string questId)
        {
            if (!_activeQuestsById.TryGetValue(questId, out var quest))
                return;

            // Force a re-check of completion/sequencing without requiring a fresh
            // ObjectiveInstance.OnCompleted event — e.g. after a save-game import mutates
            // objective progress directly.
            quest.RecheckCompletion();
        }

        public bool ClaimQuestReward(string questId)
        {
            if (!_activeQuestsById.TryGetValue(questId, out var quest))
                return false;

            if (quest.State != QuestState.Completed)
            {
                CustomLog.LogWarning($"[QuestManager] Cannot claim reward for quest '{questId}' (not completed).");
                return false;
            }

            if (quest.IsRewardClaimed)
            {
                CustomLog.LogWarning($"[QuestManager] Reward already claimed for quest '{questId}'.");
                return false;
            }

            var config = quest.Config;
            var reward = config.Reward;

            // Grant reward items
            if (reward != null && reward.Items.Length > 0)
            {
                foreach (var item in reward.Items)
                {
                    Inventory.Add(item.inventoryId, item.amount, quest.QuestId);
                }
            }

            quest.ClaimReward();

            // Fire event
            EventManager.Invoke(EventQuestRewardClaimed, questId);

            CustomLog.Log($"[QuestManager] Reward claimed for quest: {questId}");

            if (_autoSaveOnChange)
            {
                _ = SaveQuestsAsync();
            }

            return true;
        }

        public QuestInstance[] GetActiveQuests()
        {
            var quests = new QuestInstance[_activeQuestsById.Count];
            _activeQuestsById.Values.CopyTo(quests, 0);
            return quests;
        }

        public async UniTask<bool> SaveQuestsAsync()
        {
            if (!_isInitialized) return false;

            try
            {
                // Snapshot before awaiting: AcceptQuest/AbandonQuest could otherwise mutate
                // _activeQuestsById while this loop is suspended on an await, throwing
                // InvalidOperationException.
                var snapshot = new List<KeyValuePair<string, QuestInstance>>(_activeQuestsById);

                foreach (var questEntry in snapshot)
                {
                    var questId = questEntry.Key;
                    var quest = questEntry.Value;

                    var questSaveData = new QuestSaveData
                    {
                        QuestId = questId,
                        State = quest.State,
                        ObjectiveProgress = quest.SaveObjectiveProgress(),
                        RewardClaimed = quest.IsRewardClaimed
                    };
                    await UserDataManager.Instance.SetAsync(questId, questSaveData);
                }

                return true;
            }
            catch (Exception ex)
            {
                CustomLog.LogException(ex);
                return false;
            }
        }

        public QuestState GetQuestState(string questId)
        {
            if (_activeQuestsById.TryGetValue(questId, out var quest))
                return quest.State;

            return QuestState.Inactive;
        }

        // ── Private Methods ────────────────────────────────────────

        private void LoadQuestConfigs()
        {
            _configQuestsById.Clear();

            var allConfigs = ConfigManager.GetAll<ConfigQuest>();
            foreach (var config in allConfigs)
            {
                if (config == null || string.IsNullOrEmpty(config.Id)) continue;

                _configQuestsById[config.Id] = config;
            }

            CustomLog.Log($"[QuestManager] Loaded {_configQuestsById.Count} quest configs.");
        }

        private async UniTask LoadSavedQuestState()
        {
            try
            {
                var allQuestSaveData = await UserDataManager.Instance.GetAllAsync<QuestSaveData>();

                foreach (var saveData in allQuestSaveData.Values)
                {
                    if (!_configQuestsById.TryGetValue(saveData.QuestId, out var config))
                        continue;

                    var instance = new QuestInstance(config);
                    instance.LoadState(saveData.State, saveData.ObjectiveProgress, saveData.RewardClaimed);
                    instance.Activate();

                    SubscribeToQuestEvents(instance);

                    // Only listen for further progress on quests that can still make progress.
                    if (instance.State == QuestState.Active)
                    {
                        instance.Subscribe();
                    }

                    _activeQuestsById[saveData.QuestId] = instance;
                }

                CustomLog.Log($"[QuestManager] Loaded {_activeQuestsById.Count} active quests from save data.");
            }
            catch (Exception ex)
            {
                CustomLog.LogException(ex);
            }
        }

        private void SubscribeToQuestEvents(QuestInstance quest)
        {
            quest.OnStateChangedEvent.AddListener((instance, state) =>
            {
                if (state == QuestState.Completed)
                {
                    // No further objective progress can occur once completed.
                    instance.Unsubscribe();

                    EventManager.Invoke(EventQuestCompleted, instance.QuestId);
                    if (_autoSaveOnChange)
                    {
                        _ = SaveQuestsAsync();
                    }
                }
            });

            foreach (var objective in quest.Objectives)
            {
                objective.OnProgressChanged += (objInstance, oldAmount, newAmount) =>
                {
                    EventManager.Invoke(EventObjectiveProgressChanged,
                        quest.QuestId,
                        objInstance.ObjectiveId,
                        newAmount,
                        objInstance.RequiredAmount);
                };
            }

            quest.OnObjectiveCompletedEvent.AddListener((objective) =>
            {
                CustomLog.Log($"[QuestManager] Objective completed: {objective.ObjectiveId} in quest {quest.QuestId}");
            });

            quest.OnQuestAbandonedEvent.AddListener((instance) =>
            {
                instance.Unsubscribe();
                _activeQuestsById.Remove(instance.QuestId);
                EventManager.Invoke(EventQuestAbandoned, instance.QuestId);
                if (_autoSaveOnChange)
                {
                    _ = SaveQuestsAsync();
                }
            });
        }

        // ── Quest Auto-Unlock/Relock Flow ──────────────────────────

        /// <summary>
        /// Checks if all unlock conditions for a quest are currently met.
        /// </summary>
        private bool AreQuestConditionsMet(ConfigQuest questConfig)
        {
            if (questConfig == null || questConfig.UnlockedConditions == null)
                return true; // No conditions means always unlocked

            return questConfig.UnlockedConditions.IsMet();
        }

        /// <summary>
        /// Checks and adjusts the auto-unlock state for a specific quest.
        /// Auto-accepts the quest if conditions are met and it's not active.
        /// Auto-abandons the quest if conditions are no longer met and it's active.
        /// </summary>
        private void CheckQuestAutoUnlockState(string questId)
        {
            if (!_configQuestsById.TryGetValue(questId, out var config)) return;

            var conditionsMet = AreQuestConditionsMet(config);
            var isActive = _activeQuestsById.ContainsKey(questId);

            if (conditionsMet && !isActive)
            {
                // Auto-accept the quest
                CustomLog.Log($"[QuestManager] Auto-unlocking quest: {questId}");
                AcceptQuest(questId);
            }
            else if (!conditionsMet && isActive)
            {
                // Auto-abandon the quest
                CustomLog.Log($"[QuestManager] Auto-relocking quest: {questId}");
                AbandonQuest(questId);
            }
        }

        /// <summary>
        /// Checks and adjusts auto-unlock state for all registered quests.
        /// </summary>
        private void CheckAllQuestsAutoUnlockState()
        {
            foreach (var questId in _configQuestsById.Keys)
            {
                CheckQuestAutoUnlockState(questId);
            }
        }

        /// <summary>
        /// Subscribes to UserData changes to trigger re-evaluation of quest unlock conditions.
        /// </summary>
        private void SubscribeToUserDataChanges()
        {
            EventManager.Add<UserDataSaveEvent>(UserDataManager.EventUserDataSave, (evt) =>
            {
                // When user data changes, re-check all quest conditions
                CheckAllQuestsAutoUnlockState();
            });

            EventManager.Add<UserDataDeleteEvent>(UserDataManager.EventUserDataDelete, (evt) =>
            {
                // When user data is deleted, re-check all quest conditions
                CheckAllQuestsAutoUnlockState();
            });
        }

        public override string ToString() => $"QuestManager: {_activeQuestsById.Count} active quests";
    }

    // ── Serialization Helpers ────────────────────────────────────────

    [Serializable]
    public class QuestSaveData
    {
        public string QuestId;
        public QuestState State;
        public int[] ObjectiveProgress;
        public bool RewardClaimed;
    }
}
