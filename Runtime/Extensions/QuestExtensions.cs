using SiPVLib.Quest.Runtime;
using UnityEngine;

namespace SiPVLib.Quest.Extensions
{
    /// <summary>
    /// Extension methods for quest-related classes to improve usability.
    /// </summary>
    public static class QuestExtensions
    {
        /// <summary>
        /// Gets a formatted progress string for UI display.
        /// Example: "3 / 5" or "Progress: 60%"
        /// </summary>
        public static string GetProgressString(this ObjectiveInstance objective, bool showPercent = false)
        {
            if (showPercent)
                return $"{objective.Progress * 100:F0}%";
            else
                return $"{objective.CurrentAmount} / {objective.RequiredAmount}";
        }

        /// <summary>
        /// Gets a formatted progress string for entire quest.
        /// Shows completed objectives / total objectives.
        /// </summary>
        public static string GetQuestProgressString(this QuestInstance quest)
        {
            int completedCount = 0;
            foreach (var obj in quest.Objectives)
            {
                if (obj.IsCompleted) completedCount++;
            }
            return $"{completedCount} / {quest.Objectives.Length}";
        }

        /// <summary>
        /// Gets a color based on objective completion (for UI).
        /// </summary>
        public static Color GetProgressColor(this ObjectiveInstance objective)
        {
            if (objective.IsCompleted)
                return Color.green;
            else if (objective.Progress > 0.5f)
                return Color.yellow;
            else
                return Color.white;
        }

        /// <summary>
        /// Gets all active objectives for display (considers execution mode).
        /// </summary>
        public static ObjectiveInstance[] GetDisplayObjectives(this QuestInstance quest)
        {
            return quest.GetActiveObjectives();
        }

        /// <summary>
        /// Helper to check if quest can be abandoned by player.
        /// </summary>
        public static bool CanAbandon(this QuestInstance quest)
        {
            return quest.State == Enums.QuestState.Active;
        }

        /// <summary>
        /// Helper to check if reward can be claimed.
        /// </summary>
        public static bool CanClaimReward(this QuestInstance quest)
        {
            return quest.State == Enums.QuestState.Completed && !quest.IsRewardClaimed;
        }
    }
}

