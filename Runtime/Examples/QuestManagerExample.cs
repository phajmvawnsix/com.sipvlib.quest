using UnityEngine;
using Cysharp.Threading.Tasks;
using SiPVLib.Quest.Events;
using SiPVLib.Quest.Extensions;
using SiPVLib.Event;
using SiPVLib.UserData.InventoryHelper;

namespace SiPVLib.Quest.Examples
{
    /// <summary>
    /// Example MonoBehaviour demonstrating how to use the Quest system.
    /// This shows initialization, quest acceptance, event broadcasting, and reward claiming.
    /// </summary>
    public class QuestManagerExample : MonoBehaviour
    {
        // ── Example: Initialize Quest Manager ──────────────────────────

        public async void InitializeQuests()
        {
            // Initialize the QuestManager
            var initialized = await QuestManager.Instance.Initialize();

            if (!initialized)
            {
                Debug.LogError("[QuestManagerExample] Failed to initialize QuestManager");
                return;
            }

            Debug.Log("[QuestManagerExample] QuestManager initialized successfully");

            // Subscribe to quest events for UI updates
            SubscribeToQuestEvents();
        }

        // ── Example: Subscribe to Quest Events ──────────────────────────

        private void SubscribeToQuestEvents()
        {
            // Listen for quest acceptance
            EventManager.Add<string>(QuestManager.EventQuestAccepted, HandleQuestAccepted);

            // Listen for objective progress changes
            EventManager.Add(QuestManager.EventObjectiveProgressChanged, HandleObjectiveProgressChanged);

            // Listen for quest completion
            EventManager.Add<string>(QuestManager.EventQuestCompleted, HandleQuestCompleted);

            // Listen for reward claiming
            EventManager.Add<string>(QuestManager.EventQuestRewardClaimed, HandleRewardClaimed);

            // Listen for quest abandonment
            EventManager.Add<string>(QuestManager.EventQuestAbandoned, HandleQuestAbandoned);
        }

        private void HandleQuestAccepted(string questId)
        {
            Debug.Log($"[UI] Quest accepted: {questId}");
            // TODO: Show quest notification/UI panel
        }

        private void HandleObjectiveProgressChanged(params object[] parameters)
        {
            if (parameters.Length >= 4)
            {
                var questId = parameters[0] as string;
                var objectiveId = parameters[1] as string;
                var currentAmount = (int)parameters[2];
                var requiredAmount = (int)parameters[3];

                Debug.Log($"[UI] Progress: {questId}/{objectiveId} = {currentAmount}/{requiredAmount}");
                // TODO: Update progress bars/text on UI
            }
        }

        private void HandleQuestCompleted(string questId)
        {
            Debug.Log($"[UI] Quest completed: {questId}");
            // TODO: Show completion notification, enable reward claiming button
        }

        private void HandleRewardClaimed(string questId)
        {
            Debug.Log($"[UI] Reward claimed: {questId}");
            // TODO: Show reward items, update inventory display
        }

        private void HandleQuestAbandoned(string questId)
        {
            Debug.Log($"[UI] Quest abandoned: {questId}");
            // TODO: Remove quest from active quests display
        }

        // ── Example: Accept a Quest ────────────────────────────────────

        public void AcceptQuest(string questId)
        {
            if (!QuestManager.Instance.IsInitialized)
            {
                Debug.LogWarning("QuestManager not initialized yet");
                return;
            }

            var questInstance = QuestManager.Instance.AcceptQuest(questId);

            if (questInstance == null)
            {
                Debug.LogWarning($"Failed to accept quest: {questId}");
                return;
            }

            Debug.Log($"Quest accepted: {questInstance.Config.Title}");
            Debug.Log($"Description: {questInstance.Config.Description}");
            Debug.Log($"Objectives: {questInstance.Objectives.Length}");

            // Show quest details in UI
            foreach (var objective in questInstance.Objectives)
            {
                Debug.Log($"  - {objective.Title} ({objective.CurrentAmount}/{objective.RequiredAmount})");
            }
        }

        // ── Example: Game Event Broadcasting ───────────────────────────
        // Receive/Consume/WatchAds objectives now progress automatically off Inventory's and
        // AdsManager's own EventManager broadcasts — there is nothing to call manually for
        // them beyond the normal gameplay call (Inventory.Add/Remove, AdsManager.ShowRewardedAd).
        // EnemyKill has no producing SiPVLib module, so gameplay code still calls
        // EventManager.Invoke(QuestEvents.EventEnemyKilled, ...) directly.

        public void OnPlayerCollectedItem(string itemId, long amount)
        {
            Debug.Log($"[Game] Collected {amount}x {itemId}");
            Inventory.Add(itemId, amount, "gameplay");
        }

        public void OnPlayerConsumedItem(string itemId, long amount)
        {
            Debug.Log($"[Game] Consumed {amount}x {itemId}");
            Inventory.Remove(itemId, amount, "gameplay");
        }

        public void OnEnemyKilled(string enemyTypeId = null)
        {
            Debug.Log($"[Game] Enemy killed: {enemyTypeId ?? "(any)"}");
            EventManager.Invoke(QuestEvents.EventEnemyKilled,
                new QuestEvents.EnemyKilledEvent { enemyTypeId = enemyTypeId, amount = 1 });
        }

        // ── Example: Get Active Quest Info ─────────────────────────────

        public void DisplayActiveQuests()
        {
            var activeQuests = QuestManager.Instance.GetActiveQuests();

            if (activeQuests.Length == 0)
            {
                Debug.Log("No active quests");
                return;
            }

            foreach (var quest in activeQuests)
            {
                Debug.Log($"\n=== {quest.Config.Title} ===");
                Debug.Log($"State: {quest.State}");
                Debug.Log($"Progress: {quest.GetQuestProgressString()}");

                var activeObjectives = quest.GetActiveObjectives();
                foreach (var objective in activeObjectives)
                {
                    var progressStr = objective.GetProgressString();
                    var color = objective.GetProgressColor();
                    Debug.Log($"  [{objective.ObjectiveId}] {objective.Title}: {progressStr}");
                }
            }
        }

        // ── Example: Claim Quest Reward ────────────────────────────────

        public void ClaimQuestReward(string questId)
        {
            var quest = QuestManager.Instance.GetActiveQuest(questId);

            if (quest == null)
            {
                Debug.LogWarning($"Quest not found: {questId}");
                return;
            }

            if (!quest.CanClaimReward())
            {
                Debug.LogWarning($"Cannot claim reward for {questId} (state: {quest.State})");
                return;
            }

            var success = QuestManager.Instance.ClaimQuestReward(questId);

            if (success)
            {
                var reward = quest.Config.Reward;
                Debug.Log($"Reward claimed: {reward}");
                // TODO: Animate reward popup
            }
        }

        // ── Example: Abandon Quest ────────────────────────────────────

        public void AbandonQuest(string questId)
        {
            var quest = QuestManager.Instance.GetActiveQuest(questId);

            if (quest == null)
            {
                Debug.LogWarning($"Quest not found: {questId}");
                return;
            }

            if (!quest.CanAbandon())
            {
                Debug.LogWarning($"Cannot abandon quest in state: {quest.State}");
                return;
            }

            QuestManager.Instance.AbandonQuest(questId);
            Debug.Log($"Quest abandoned: {questId}");
        }

        // ── Example Complete Workflow ──────────────────────────────────

        public async void CompleteQuestWorkflow()
        {
            // 1. Initialize
            Debug.Log("=== Starting Complete Quest Workflow ===");
            await QuestManager.Instance.Initialize();

            // 2. Subscribe to events
            SubscribeToQuestEvents();

            // 3. Accept a quest (assume "quest_collect_apples" exists in config)
            var quest = QuestManager.Instance.AcceptQuest("quest_collect_apples");
            if (quest == null)
            {
                Debug.LogError("Failed to accept quest");
                return;
            }

            // 4. Show initial state
            Debug.Log($"Quest started: {quest.Config.Title}");
            foreach (var obj in quest.Objectives)
            {
                Debug.Log($"  Objective: {obj.Title} (x{obj.RequiredAmount})");
            }

            // 5. Simulate gameplay events
            Debug.Log("\n--- Simulating Gameplay ---");
            await UniTask.Delay(1000);
            OnPlayerCollectedItem("apple", 2L);

            await UniTask.Delay(1000);
            OnPlayerCollectedItem("apple", 3L); // Total: 5 apples = quest complete

            // 6. Check if quest is completed
            await UniTask.Delay(500);
            if (quest.IsCompleted)
            {
                Debug.Log("Quest completed!");

                // 7. Claim reward
                await UniTask.Delay(1000);
                ClaimQuestReward(quest.QuestId);
            }

            Debug.Log("=== Workflow Complete ===");
        }

        // ── Unity Lifecycle ────────────────────────────────────────────

        private void Start()
        {
            // Uncomment to run example on start:
            // InitializeQuests();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events when destroyed
            EventManager.Remove<string>(QuestManager.EventQuestAccepted, HandleQuestAccepted);
            EventManager.Remove(QuestManager.EventObjectiveProgressChanged, HandleObjectiveProgressChanged);
            EventManager.Remove<string>(QuestManager.EventQuestCompleted, HandleQuestCompleted);
            EventManager.Remove<string>(QuestManager.EventQuestRewardClaimed, HandleRewardClaimed);
            EventManager.Remove<string>(QuestManager.EventQuestAbandoned, HandleQuestAbandoned);
        }
    }
}

