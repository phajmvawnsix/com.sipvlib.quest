using System;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.Quest.Events;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Progress driven by <see cref="QuestEvents.EventEnemyKilled"/> — the one event Quest
    /// originates itself, since no SiPVLib module tracks combat kills. Gameplay code calls
    /// <c>EventManager.Invoke(QuestEvents.EventEnemyKilled, new QuestEvents.EnemyKilledEvent { ... })</c>
    /// when an enemy dies.
    /// </summary>
    public class EnemyKillObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveEnemyKill _config;
        private readonly Action<QuestEvents.EnemyKilledEvent> _onEnemyKilled;

        public EnemyKillObjectiveInstance(QuestObjectiveEnemyKill config) : base(config)
        {
            _config = config;
            _onEnemyKilled = OnEnemyKilled;
        }

        protected override void SubscribeEvents()
        {
            EventManager.Add(QuestEvents.EventEnemyKilled, _onEnemyKilled);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(QuestEvents.EventEnemyKilled, _onEnemyKilled);
        }

        private void OnEnemyKilled(QuestEvents.EnemyKilledEvent evt)
        {
            if (!string.IsNullOrEmpty(_config.EnemyTypeId) && evt.enemyTypeId != _config.EnemyTypeId)
                return;

            IncreaseProgress(evt.amount > 0 ? evt.amount : 1);
        }
    }
}
