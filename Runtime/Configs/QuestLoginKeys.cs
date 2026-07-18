namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Shared UserData feature-counter id convention for login-based objectives
    /// (<see cref="QuestObjectiveLoginOnDay"/>, <see cref="QuestObjectiveLoginStreak"/>).
    /// Game code must call <c>UserDataManager.Instance.IncrementFeatureAmountCounter(DefaultFeatureId)</c>
    /// once per calendar day at login for these objective types to progress.
    /// </summary>
    public static class QuestLoginKeys
    {
        public const string DefaultFeatureId = "login";
    }
}
