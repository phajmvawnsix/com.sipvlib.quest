using System;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.UserData;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Pull/state-query objective: progress equals the delta of a UserData feature counter
    /// since this objective started listening. Game code must call
    /// <c>UserDataManager.Instance.IncrementFeatureAmountCounter(featureId)</c> at the moment
    /// of the visit — this objective only observes the counter changing via
    /// <see cref="UserDataManager.EventUserDataSave"/>, it does not detect visits itself.
    /// </summary>
    public class VisitFeatureObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveVisitFeature _config;
        private readonly Action<UserDataSaveEvent> _onUserDataSave;

        public VisitFeatureObjectiveInstance(QuestObjectiveVisitFeature config) : base(config)
        {
            _config = config;
            _onUserDataSave = OnUserDataSave;
        }

        protected override void SubscribeEvents()
        {
            var baseline = UserDataManager.Instance != null
                ? UserDataManager.Instance.GetFeatureAmountCounter(_config.FeatureId).totalCounter
                : 0;
            CaptureBaseline(baseline);

            EventManager.Add(UserDataManager.EventUserDataSave, _onUserDataSave);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(UserDataManager.EventUserDataSave, _onUserDataSave);
        }

        private void OnUserDataSave(UserDataSaveEvent evt)
        {
            if (evt.key != _config.FeatureId) return;

            var total = UserDataManager.Instance.GetFeatureAmountCounter(_config.FeatureId).totalCounter;
            ApplyProgressFromBaseline(total);
        }
    }
}
