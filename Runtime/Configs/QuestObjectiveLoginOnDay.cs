using SiPVLib.Quest.Enums;
using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: log in on a specific day of the week or day of the month.
    /// Example: "Log in on a Monday"
    /// Triggered by a UserData save on the login feature counter, matched against
    /// <see cref="SiPVLib.AntiCheat.GameTime.Now"/>. Game code must call
    /// <c>UserDataManager.Instance.IncrementFeatureAmountCounter(loginFeatureId)</c> once per
    /// calendar day. See <see cref="Runtime.ObjectiveInstances.LoginOnDayObjectiveInstance"/>.
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveLoginOnDay : QuestObjective
    {
        [SerializeField]
        [Tooltip("UserData feature-counter id incremented once per calendar day at login.")]
        protected string _loginFeatureId = QuestLoginKeys.DefaultFeatureId;

        [SerializeField]
        [Tooltip("Whether _targetDayValue is a day of week (0-6) or day of month (1-31).")]
        protected DayMatchMode _mode = DayMatchMode.DayOfWeek;

        [SerializeField]
        [Tooltip("Target value: 0-6 for DayOfWeek (Sunday=0), 1-31 for DayOfMonth.")]
        protected int _targetDayValue;

        public string LoginFeatureId => _loginFeatureId;
        public DayMatchMode Mode => _mode;
        public int TargetDayValue => _targetDayValue;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.LoginOnDayObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Login on {_mode} {_targetDayValue}";
    }
}
