using UnityEngine;

namespace SiPVLib.Quest.Configs
{
    /// <summary>
    /// Objective: purchase a specific shop item X times.
    /// Example: "Buy the starter pack"
    /// Progress is driven by <c>ShopManager{T}.EventShopResult</c> (see
    /// <see cref="Runtime.ObjectiveInstances.BuyShopItemObjectiveInstance"/>).
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveBuyShopItem : QuestObjective
    {
        [SerializeField]
        [Tooltip("The shop item id that must be purchased.")]
        protected string _itemId;

        public string ItemId => _itemId;

        public override Runtime.ObjectiveInstance CreateInstance()
        {
            return new Runtime.ObjectiveInstances.BuyShopItemObjectiveInstance(this);
        }

        public override string ToString() => $"{base.ToString()} | Buy Item: {_itemId}";
    }
}
