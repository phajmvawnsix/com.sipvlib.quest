namespace SiPVLib.Quest.Events
{
    /// <summary>
    /// Event constants and payloads that the Quest module itself originates.
    /// Every other objective type consumes events already broadcast by the
    /// owning SiPVLib module (Inventory, ShopManager, AdsManager, UserDataManager)
    /// via <see cref="SiPVLib.Event.EventManager"/> instead of duplicating them here.
    /// </summary>
    public static class QuestEvents
    {
        /// <summary>
        /// Fired by gameplay code when an enemy is killed. No SiPVLib module tracks
        /// combat kills, so this is the one event Quest must originate itself.
        /// </summary>
        public const string EventEnemyKilled = "Quest.EnemyKilled";

        public struct EnemyKilledEvent
        {
            public string enemyTypeId;
            public int amount;
        }
    }
}
