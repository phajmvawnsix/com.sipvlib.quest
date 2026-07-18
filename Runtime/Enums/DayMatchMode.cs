namespace SiPVLib.Quest.Enums
{
    /// <summary>
    /// How <see cref="Configs.QuestObjectiveLoginOnDay"/> matches the current date.
    /// </summary>
    public enum DayMatchMode
    {
        /// <summary>Target value is 0-6 (System.DayOfWeek).</summary>
        DayOfWeek,

        /// <summary>Target value is 1-31 (calendar day of month).</summary>
        DayOfMonth
    }
}
