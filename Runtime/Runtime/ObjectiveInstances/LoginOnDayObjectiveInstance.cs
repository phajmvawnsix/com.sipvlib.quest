using System;
using SiPVLib.AntiCheat;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.Quest.Enums;
using SiPVLib.UserData;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Push objective: on each login-feature-counter save, checks whether today matches the
    /// configured day-of-week/day-of-month and increases progress once per qualifying day.
    /// Game code must call
    /// <c>UserDataManager.Instance.IncrementFeatureAmountCounter(loginFeatureId)</c> once per
    /// calendar day — <see cref="UserData.FeatureAmountCounter"/> does not enforce that itself.
    /// </summary>
    public class LoginOnDayObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveLoginOnDay _config;
        private readonly Action<UserDataSaveEvent> _onUserDataSave;

        public LoginOnDayObjectiveInstance(QuestObjectiveLoginOnDay config) : base(config)
        {
            _config = config;
            _onUserDataSave = OnUserDataSave;
        }

        protected override void SubscribeEvents()
        {
            EventManager.Add(UserDataManager.EventUserDataSave, _onUserDataSave);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(UserDataManager.EventUserDataSave, _onUserDataSave);
        }

        private void OnUserDataSave(UserDataSaveEvent evt)
        {
            if (evt.key != _config.LoginFeatureId) return;

            var now = GameTime.Now;
            var matches = _config.Mode == DayMatchMode.DayOfWeek
                ? (int)now.DayOfWeek == _config.TargetDayValue
                : now.Day == _config.TargetDayValue;

            if (matches)
            {
                IncreaseProgress(1);
            }
        }
    }
}
