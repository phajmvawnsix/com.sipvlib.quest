using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: Consume X items of a specific type.
    /// Example: "Use 5 health potions"
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveConsume : QuestObjective
    {
        [SerializeField]
        [Tooltip("The item ID that must be consumed.")]
        protected string _itemId;

        public string ItemId => _itemId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.ConsumeObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Item: {_itemId}";
    }
}

