using System;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.Shop;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Progress driven by <c>ShopManager{UnityShopManager}.EventShopResult</c>. The per-item
    /// target key on that event (<c>ShopManager{T}.GetTargetId</c>) is private, so this
    /// subscribes globally and filters by item id/action/success instead.
    /// </summary>
    public class BuyShopItemObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveBuyShopItem _config;
        private readonly Action<ShopResultEvent> _onShopResult;

        public BuyShopItemObjectiveInstance(QuestObjectiveBuyShopItem config) : base(config)
        {
            _config = config;
            _onShopResult = OnShopResult;
        }

        protected override void SubscribeEvents()
        {
            EventManager.Add(ShopManager<UnityShopManager>.EventShopResult, _onShopResult);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(ShopManager<UnityShopManager>.EventShopResult, _onShopResult);
        }

        private void OnShopResult(ShopResultEvent evt)
        {
            if (!evt.success || evt.actionType != ShopActionType.Purchase) return;
            if (evt.itemId != _config.ItemId) return;

            IncreaseProgress(1);
        }
    }
}
