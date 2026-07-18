using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: log in on N distinct days.
    /// Example: "Log in 7 times"
    /// <b>Known limitation:</b> this counts cumulative distinct login-increments, NOT a true
    /// consecutive-day streak with reset-on-miss — no SiPVLib module tracks streak-with-break,
    /// and this module deliberately does not invent new persistence for it. Progress is a
    /// pull/state-query against the login UserData feature counter; game code must call
    /// <c>UserDataManager.Instance.IncrementFeatureAmountCounter(loginFeatureId)</c> once per
    /// calendar day. See <see cref="Runtime.ObjectiveInstances.LoginStreakObjectiveInstance"/>.
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveLoginStreak : QuestObjective
    {
        [SerializeField]
        [Tooltip("UserData feature-counter id incremented once per calendar day at login.")]
        protected string _loginFeatureId = QuestLoginKeys.DefaultFeatureId;

        public string LoginFeatureId => _loginFeatureId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.LoginStreakObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Login Streak (cumulative days)";
    }
}
