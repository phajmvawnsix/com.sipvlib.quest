using SiPVLib.Quest.Enums;
using SiPVLib.Quest.Configs;
using SiPVLib.Quest.Runtime;

namespace SiPVLib.Quest.Interfaces
{
    /// <summary>
    /// Represents a complete quest configuration and runtime state wrapper.
    /// A quest contains one or more objectives and offers rewards upon completion.
    /// </summary>
    public interface IQuest
    {
        /// <summary>
        /// Gets the config that defines this quest.
        /// </summary>
        ConfigQuest Config { get; }

        /// <summary>
        /// Gets the current state of this quest instance.
        /// </summary>
        QuestState State { get; }

        /// <summary>
        /// Gets the objectives that make up this quest.
        /// </summary>
        ObjectiveInstance[] Objectives { get; }

        /// <summary>
        /// Gets the execution mode (Sequential or Parallel) for this quest's objectives.
        /// </summary>
        ObjectiveExecutionMode ExecutionMode { get; }

        /// <summary>
        /// Checks if this quest is fully completed (all objectives done).
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets the progress of the first active objective (0-1 range).
        /// Used for UI display and diagnostics.
        /// </summary>
        float GetCurrentProgress();

        /// <summary>
        /// Handles an objective completion. Updates quest state if needed.
        /// </summary>
        void OnObjectiveCompleted(ObjectiveInstance completedObjective);

        /// <summary>
        /// Marks this quest as abandoned.
        /// </summary>
        void Abandon();

        /// <summary>
        /// Marks this quest as failed.
        /// </summary>
        void Fail();
    }
}

