namespace SiPVLib.Quest.Enums
{
    /// <summary>
    /// Determines how objectives in a quest are executed.
    /// </summary>
    public enum ObjectiveExecutionMode
    {
        /// <summary>
        /// Objectives must be completed one after another.
        /// Objective N+1 is activated only after objective N is completed.
        /// </summary>
        Sequential,

        /// <summary>
        /// All objectives are active simultaneously.
        /// Quest completes when all objectives are done.
        /// </summary>
        Parallel
    }
}

