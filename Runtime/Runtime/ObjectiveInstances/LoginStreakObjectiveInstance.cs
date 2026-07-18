using System;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.UserData;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Pull/state-query objective: progress equals the delta of the login UserData feature
    /// counter's <c>totalCounter</c> since this objective started listening.
    /// <b>Known limitation:</b> this counts cumulative distinct login-increments, NOT a true
    /// consecutive-day streak with reset-on-miss — see
    /// <see cref="Configs.QuestObjectiveLoginStreak"/> for details.
    /// </summary>
    public class LoginStreakObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveLoginStreak _config;
        private readonly Action<UserDataSaveEvent> _onUserDataSave;

        public LoginStreakObjectiveInstance(QuestObjectiveLoginStreak config) : base(config)
        {
            _config = config;
            _onUserDataSave = OnUserDataSave;
        }

        protected override void SubscribeEvents()
        {
            var baseline = UserDataManager.Instance != null
                ? UserDataManager.Instance.GetFeatureAmountCounter(_config.LoginFeatureId).totalCounter
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
            if (evt.key != _config.LoginFeatureId) return;

            var total = UserDataManager.Instance.GetFeatureAmountCounter(_config.LoginFeatureId).totalCounter;
            ApplyProgressFromBaseline(total);
        }
    }
}
