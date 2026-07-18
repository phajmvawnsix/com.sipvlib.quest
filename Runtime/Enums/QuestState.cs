namespace SiPVLib.Quest.Enums
{
    /// <summary>
    /// Represents the current state of a quest instance.
    /// </summary>
    public enum QuestState
    {
        /// <summary>
        /// Quest is not yet accepted by the player.
        /// </summary>
        Inactive,

        /// <summary>
        /// Quest is currently active and being pursued by the player.
        /// </summary>
        Active,

        /// <summary>
        /// Quest has been successfully completed (but reward may not be claimed yet).
        /// </summary>
        Completed,

        /// <summary>
        /// Quest has failed and cannot be resumed (if repeatable, can be reaccepted).
        /// </summary>
        Failed,

        /// <summary>
        /// Player has voluntarily abandoned the quest.
        /// </summary>
        Abandoned
    }
}

