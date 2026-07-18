using Cysharp.Threading.Tasks;
using SiPVLib.Quest.Enums;
using SiPVLib.Quest.Runtime;

namespace SiPVLib.Quest.Interfaces
{
    /// <summary>
    /// Central interface for managing quests in the game.
    /// Handles accepting, abandoning, checking completion, and persistence.
    /// </summary>
    public interface IQuestManager
    {
        /// <summary>
        /// Initializes the quest manager and loads persisted quest data.
        /// </summary>
        UniTask<bool> Initialize();

        /// <summary>
        /// Checks if the manager is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Accepts a quest by ID. Returns the created QuestInstance or null if failed.
        /// </summary>
        /// <param name="questId">The config ID of the quest to accept.</param>
        /// <returns>The active QuestInstance, or null if the quest could not be accepted.</returns>
        QuestInstance AcceptQuest(string questId);

        /// <summary>
        /// Abandons an active quest. Does nothing if quest is not active.
        /// </summary>
        /// <param name="questId">The config ID of the quest to abandon.</param>
        void AbandonQuest(string questId);

        /// <summary>
        /// Gets the current instance of a quest by ID.
        /// Returns null if quest is not active.
        /// </summary>
        /// <param name="questId">The config ID of the quest.</param>
        /// <returns>The QuestInstance or null.</returns>
        QuestInstance GetActiveQuest(string questId);

        /// <summary>
        /// Manually checks and updates quest completion status.
        /// Called automatically when objectives complete, but can be called manually.
        /// </summary>
        /// <param name="questId">The config ID of the quest to check.</param>
        void CheckQuestCompletion(string questId);

        /// <summary>
        /// Claims the reward for a completed quest.
        /// Returns false if quest is not in Completed state.
        /// </summary>
        /// <param name="questId">The config ID of the quest.</param>
        /// <returns>True if reward was successfully claimed.</returns>
        bool ClaimQuestReward(string questId);

        /// <summary>
        /// Gets all currently active quests.
        /// </summary>
        /// <returns>Array of active QuestInstances.</returns>
        QuestInstance[] GetActiveQuests();

        /// <summary>
        /// Saves all quest progress to persistent storage.
        /// </summary>
        UniTask<bool> SaveQuestsAsync();

        /// <summary>
        /// Gets the state of a quest (or QuestState.Inactive if not found).
        /// </summary>
        QuestState GetQuestState(string questId);
    }
}

