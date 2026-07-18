using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: use/visit a feature X times.
    /// Example: "Open the shop screen 3 times"
    /// Progress is a pull/state-query against a UserData feature counter, not something
    /// this module tracks itself — game code must call
    /// <c>UserDataManager.Instance.IncrementFeatureAmountCounter(featureId)</c> at the moment
    /// of the visit. See <see cref="Runtime.ObjectiveInstances.VisitFeatureObjectiveInstance"/>.
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveVisitFeature : QuestObjective
    {
        [SerializeField]
        [Tooltip("The UserData feature-counter id incremented when the feature is visited.")]
        protected string _featureId;

        public string FeatureId => _featureId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.VisitFeatureObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Visit Feature: {_featureId}";
    }
}
