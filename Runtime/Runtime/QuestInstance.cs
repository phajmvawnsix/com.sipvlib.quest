using System;
using System.Collections.Generic;
using SiPVLib.Quest.Configs;
using SiPVLib.Quest.Enums;
using SiPVLib.Quest.Interfaces;
using UnityEngine.Events;

namespace SiPVLib.Quest.Runtime
{
    /// <summary>
    /// Runtime instance of a quest, wrapping a ConfigQuest and managing its state.
    /// Handles objective progression and quest completion logic.
    /// </summary>
    public class QuestInstance : IQuest
    {
        private readonly ConfigQuest _config;
        private QuestState _state;
        private readonly ObjectiveInstance[] _objectives;
        private int _currentObjectiveIndex;
        private bool _isRewardClaimed;

        /// <summary>
        /// Fired when the quest state changes.
        /// </summary>
        public UnityEvent<QuestInstance, QuestState> OnStateChangedEvent;

        /// <summary>
        /// Fired when an objective is completed.
        /// </summary>
        public UnityEvent<ObjectiveInstance> OnObjectiveCompletedEvent;

        /// <summary>
        /// Fired when the entire quest is completed.
        /// </summary>
        public UnityEvent<QuestInstance> OnQuestCompletedEvent;

        /// <summary>
        /// Fired when the quest is abandoned.
        /// </summary>
        public UnityEvent<QuestInstance> OnQuestAbandonedEvent;

        // ── IQuest Implementation ────────────────────────────────────

        public ConfigQuest Config => _config;
        public QuestState State => _state;
        public ObjectiveInstance[] Objectives => _objectives;
        public ObjectiveExecutionMode ExecutionMode => _config.ExecutionMode;
        public bool IsCompleted => _state == QuestState.Completed;

        // ── Properties ────────────────────────────────────────────

        public string QuestId => _config.Id;
        public bool IsRewardClaimed => _isRewardClaimed;

        public QuestInstance(ConfigQuest config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _state = QuestState.Inactive;
            _currentObjectiveIndex = 0;
            _isRewardClaimed = false;

            // Create objective instances from config
            var objectiveConfigs = config.Objectives;
            _objectives = new ObjectiveInstance[objectiveConfigs.Length];
            for (int i = 0; i < objectiveConfigs.Length; i++)
            {
                _objectives[i] = objectiveConfigs[i].CreateInstance();
                _objectives[i].OnCompleted += HandleObjectiveCompleted;
            }
        }

        /// <summary>
        /// Activates the quest (transitions from Inactive to Active).
        /// </summary>
        public void Activate()
        {
            if (_state != QuestState.Inactive) return;

            _state = QuestState.Active;
            _currentObjectiveIndex = 0;
            OnStateChangedEvent?.Invoke(this, _state);

            // Activate first objective(s)
            if (ExecutionMode == ObjectiveExecutionMode.Sequential)
            {
                // Activate only the first objective
                // (The objective is already "active" in our model)
            }
            else if (ExecutionMode == ObjectiveExecutionMode.Parallel)
            {
                // All objectives are active from the start
            }
        }

        /// <summary>
        /// Subscribes every objective's progress-tracking event listener(s).
        /// Call after <see cref="Activate"/> when the quest becomes/stays active.
        /// </summary>
        public void Subscribe()
        {
            foreach (var objective in _objectives)
            {
                objective.Subscribe();
            }
        }

        /// <summary>
        /// Unsubscribes every objective's event listener(s). Must be called whenever this
        /// quest instance is abandoned, completed, or otherwise stops tracking progress,
        /// to avoid leaking listeners for a dead quest.
        /// </summary>
        public void Unsubscribe()
        {
            foreach (var objective in _objectives)
            {
                objective.Unsubscribe();
            }
        }

        /// <summary>
        /// Gets the current active objective (for Sequential mode).
        /// Returns null if no objectives remain.
        /// </summary>
        public ObjectiveInstance GetCurrentObjective()
        {
            if (_currentObjectiveIndex < _objectives.Length)
                return _objectives[_currentObjectiveIndex];
            return null;
        }

        /// <summary>
        /// Gets all currently active objectives.
        /// For Sequential: only the current one. For Parallel: all incomplete ones.
        /// </summary>
        public ObjectiveInstance[] GetActiveObjectives()
        {
            if (ExecutionMode == ObjectiveExecutionMode.Sequential)
            {
                var current = GetCurrentObjective();
                return current != null ? new[] { current } : Array.Empty<ObjectiveInstance>();
            }
            else
            {
                var active = new List<ObjectiveInstance>();
                foreach (var obj in _objectives)
                {
                    if (!obj.IsCompleted)
                        active.Add(obj);
                }
                return active.ToArray();
            }
        }

        public float GetCurrentProgress()
        {
            var activeObjectives = GetActiveObjectives();
            if (activeObjectives.Length == 0)
                return 1f;

            float totalProgress = 0f;
            foreach (var obj in activeObjectives)
            {
                totalProgress += obj.Progress;
            }
            return totalProgress / activeObjectives.Length;
        }

        public void OnObjectiveCompleted(ObjectiveInstance completedObjective)
        {
            if (completedObjective == null) return;

            OnObjectiveCompletedEvent?.Invoke(completedObjective);

            RecheckCompletion();
        }

        /// <summary>
        /// Re-evaluates completion/sequencing from current objective state without requiring
        /// a fresh <see cref="ObjectiveInstance.OnCompleted"/> event. Used by the completion
        /// handler above and exposed externally via <c>QuestManager.CheckQuestCompletion</c>
        /// for forced re-checks (e.g. after a save-game import mutates progress directly).
        /// </summary>
        public void RecheckCompletion()
        {
            if (AllObjectivesCompleted())
            {
                CompleteQuest();
            }
            else if (ExecutionMode == ObjectiveExecutionMode.Sequential)
            {
                // Move to next objective
                var current = GetCurrentObjective();
                if (current != null && current.IsCompleted)
                {
                    _currentObjectiveIndex++;
                }
            }
        }

        public void Abandon()
        {
            if (_state == QuestState.Inactive || _state == QuestState.Abandoned)
                return;

            _state = QuestState.Abandoned;
            OnStateChangedEvent?.Invoke(this, _state);
            OnQuestAbandonedEvent?.Invoke(this);
        }

        public void Fail()
        {
            if (_state == QuestState.Inactive || _state == QuestState.Completed || _state == QuestState.Failed)
                return;

            _state = QuestState.Failed;
            OnStateChangedEvent?.Invoke(this, _state);
        }

        /// <summary>
        /// Marks the quest as completed and fires OnQuestCompleted event.
        /// </summary>
        private void CompleteQuest()
        {
            if (_state == QuestState.Completed)
                return;

            _state = QuestState.Completed;
            OnStateChangedEvent?.Invoke(this, _state);
            OnQuestCompletedEvent?.Invoke(this);
        }

        /// <summary>
        /// Checks if all objectives are completed.
        /// </summary>
        private bool AllObjectivesCompleted()
        {
            foreach (var obj in _objectives)
            {
                if (!obj.IsCompleted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Handles an objective completion event.
        /// </summary>
        private void HandleObjectiveCompleted(ObjectiveInstance objective)
        {
            OnObjectiveCompleted(objective);
        }

        /// <summary>
        /// Marks the reward as claimed.
        /// </summary>
        public void ClaimReward()
        {
            _isRewardClaimed = true;
        }

        /// <summary>
        /// Loads quest state from save data.
        /// </summary>
        public void LoadState(QuestState state, int[] objectiveProgress, bool rewardClaimed)
        {
            _state = state;
            _isRewardClaimed = rewardClaimed;

            if (objectiveProgress != null)
            {
                for (int i = 0; i < _objectives.Length && i < objectiveProgress.Length; i++)
                {
                    _objectives[i].SetProgress(objectiveProgress[i]);
                }
            }
        }

        /// <summary>
        /// Saves quest state to an array format for persistence.
        /// </summary>
        public int[] SaveObjectiveProgress()
        {
            var progress = new int[_objectives.Length];
            for (int i = 0; i < _objectives.Length; i++)
            {
                progress[i] = _objectives[i].CurrentAmount;
            }
            return progress;
        }

        public override string ToString() => $"Quest [{QuestId}] {_state}: {GetCurrentProgress() * 100:F0}%";
    }
}
