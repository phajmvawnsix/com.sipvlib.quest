using System.Linq;
using SiPVLib.Config.Configs;
using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Represents reward items granted when a quest is completed.
    /// Uses GameItem structure to define item type, ID, and amount.
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        [SerializeField]
        [Tooltip("The reward items (currency, equipment, consumables, etc.).")]
        protected GameItem[] _items;

        public GameItem[] Items => _items ?? System.Array.Empty<GameItem>();

        public override string ToString()
        {
            if (_items == null || _items.Length == 0)
                return "(No rewards)";

            return $"Reward: {string.Join(", ", _items.Select(item => item.ToString()))}";
        }
    }
}

