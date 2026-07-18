namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: Watch X ads.
    /// Example: "Watch 3 ads to earn rewards"
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveWatchAds : QuestObjective
    {
        // This objective type doesn't need additional fields currently.
        // _requiredAmount specifies the number of ads to watch.

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.WatchAdsObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Watch {RequiredAmount} ads";
    }
}

