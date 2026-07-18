using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: Receive X items of a specific type.
    /// Example: "Collect 10 gold coins"
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveReceive : QuestObjective
    {
        [SerializeField]
        [Tooltip("The item ID that must be received/collected.")]
        protected string _itemId;

        public string ItemId => _itemId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.ReceiveObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Item: {_itemId}";
    }
}

