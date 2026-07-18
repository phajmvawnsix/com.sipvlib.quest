using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Base abstract class for quest objectives.
    /// Subclasses define specific objective types (Consume, Receive, WatchAds, etc.).
    /// </summary>
    [Serializable]
    public abstract class QuestObjective
    {
        [SerializeField]
        [Tooltip("Unique identifier for this objective within its quest.")]
        protected string _objectiveId;

        [SerializeField]
        [Tooltip("Human-readable title of this objective.")]
        protected string _title;

        [SerializeField]
        [Tooltip("The amount required to complete this objective.")]
#if ODIN_INSPECTOR
        [PropertyRange(1, 10000)]
#endif
        protected int _requiredAmount = 1;

        #region Properties

        public string ObjectiveId => _objectiveId;
        public string Title => _title;
        public int RequiredAmount => _requiredAmount;

        #endregion

        /// <summary>
        /// Creates a runtime instance for this objective. Every concrete subclass overrides
        /// this to return its own dedicated <see cref="Runtime.ObjectiveInstance"/> subclass
        /// under <c>Runtime.ObjectiveInstances</c>, which owns the event subscription that
        /// drives its progress (see <see cref="Runtime.ObjectiveInstance.Subscribe"/>).
        /// </summary>
        public virtual Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstance(this);
        }

        public override string ToString() => $"{GetType().Name} [{ObjectiveId}]: {Title} (x{RequiredAmount})";
    }
}

