using System;
using SiPVLib.Quest.Configs;

namespace SiPVLib.Quest.Runtime
{
    /// <summary>
    /// Runtime instance of a quest objective, tracking progress.
    /// Wraps a QuestObjective config and maintains currentAmount.
    /// Subclasses subscribe to the relevant SiPVLib module event (Inventory, ShopManager,
    /// AdsManager, UserDataManager, or a Quest-owned event) to drive progress; see
    /// <see cref="SubscribeEvents"/>/<see cref="UnsubscribeEvents"/>.
    /// </summary>
    public class ObjectiveInstance
    {
        private readonly QuestObjective _config;
        private int _currentAmount;
        private bool _isCompleted;
        private bool _isSubscribed;

        /// <summary>Baseline value captured at <see cref="Subscribe"/> time by pull-based
        /// (state-query) subclasses, so pre-existing progress in the source counter isn't
        /// retroactively credited to a freshly accepted quest.</summary>
        private long _progressBaseline;

        /// <summary>
        /// Fired when this objective's progress changes.
        /// Parameters: (ObjectiveInstance this, int oldAmount, int newAmount)
        /// </summary>
        public event Action<ObjectiveInstance, int, int> OnProgressChanged;

        /// <summary>
        /// Fired when this objective is completed.
        /// </summary>
        public event Action<ObjectiveInstance> OnCompleted;

        // ── Properties ────────────────────────────────────────────

        public QuestObjective Config => _config;
        public string ObjectiveId => _config.ObjectiveId;
        public string Title => _config.Title;
        public int CurrentAmount => _currentAmount;
        public int RequiredAmount => _config.RequiredAmount;
        public bool IsCompleted => _isCompleted;

        /// <summary>Fraction complete. A misconfigured <c>RequiredAmount &lt;= 0</c> is
        /// treated as trivially complete rather than dividing by zero.</summary>
        public float Progress => RequiredAmount <= 0 ? 1f : (float)_currentAmount / RequiredAmount;

        public ObjectiveInstance(QuestObjective config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _currentAmount = 0;
            _isCompleted = false;
        }

        // ── Subscription lifecycle ───────────────────────────────────

        /// <summary>
        /// Registers this objective's progress-tracking event listener(s). Called by
        /// <see cref="QuestInstance.Subscribe"/> when the owning quest is accepted or
        /// restored from save data. Idempotent.
        /// </summary>
        public void Subscribe()
        {
            if (_isSubscribed) return;
            _isSubscribed = true;
            SubscribeEvents();
        }

        /// <summary>
        /// Removes this objective's event listener(s). Called by
        /// <see cref="QuestInstance.Unsubscribe"/> whenever the owning quest is abandoned,
        /// completed, or otherwise removed from the active set — must be symmetric with
        /// <see cref="Subscribe"/> to avoid leaking listeners on a dead objective.
        /// </summary>
        public void Unsubscribe()
        {
            if (!_isSubscribed) return;
            _isSubscribed = false;
            UnsubscribeEvents();
        }

        /// <summary>Override to register event listeners via <see cref="SiPVLib.Event.EventManager"/>.</summary>
        protected virtual void SubscribeEvents()
        {
        }

        /// <summary>Override to remove the exact listener(s) registered in <see cref="SubscribeEvents"/>.</summary>
        protected virtual void UnsubscribeEvents()
        {
        }

        // ── Baseline helper (for pull/state-query objective types) ──────

        /// <summary>Captures the source counter's current value so later progress reflects
        /// only the delta accumulated since the quest was accepted.</summary>
        protected void CaptureBaseline(long current) => _progressBaseline = current;

        /// <summary>Sets progress to <paramref name="current"/> minus the captured baseline.</summary>
        protected void ApplyProgressFromBaseline(long current) => SetProgress((int)(current - _progressBaseline));

        // ── Progress mutation ────────────────────────────────────────

        /// <summary>
        /// Increases the progress counter.
        /// Automatically completes the objective if threshold is reached.
        /// </summary>
        public void IncreaseProgress(int amount = 1)
        {
            if (_isCompleted) return;

            var oldAmount = _currentAmount;
            _currentAmount = Math.Min(_currentAmount + amount, RequiredAmount);

            OnProgressChanged?.Invoke(this, oldAmount, _currentAmount);

            if (_currentAmount >= RequiredAmount && !_isCompleted)
            {
                CompleteObjective();
            }
        }

        /// <summary>
        /// Resets the progress counter to 0 (does not un-complete).
        /// Used when restarting/reloading objectives.
        /// </summary>
        public void ResetProgress()
        {
            _currentAmount = 0;
            OnProgressChanged?.Invoke(this, _currentAmount, 0);
        }

        /// <summary>
        /// Marks this objective as completed and fires the OnCompleted event.
        /// </summary>
        private void CompleteObjective()
        {
            if (_isCompleted) return;

            _isCompleted = true;
            OnCompleted?.Invoke(this);
        }

        /// <summary>
        /// Sets the current progress directly (for loading from save data, or pull-based
        /// objective types recomputing progress from an external counter).
        /// </summary>
        public void SetProgress(int amount)
        {
            var oldAmount = _currentAmount;
            _currentAmount = Math.Clamp(amount, 0, RequiredAmount);

            OnProgressChanged?.Invoke(this, oldAmount, _currentAmount);

            if (_currentAmount >= RequiredAmount && !_isCompleted)
            {
                CompleteObjective();
            }
        }

        public override string ToString() => $"[{ObjectiveId}] {Title} ({CurrentAmount}/{RequiredAmount})";
    }
}
