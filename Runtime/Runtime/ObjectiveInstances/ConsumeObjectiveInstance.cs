using System;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;
using SiPVLib.UserData.InventoryHelper;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Progress driven by <see cref="Inventory.EventInventoryChange"/> (Remove operations,
    /// targeted to the configured item key).
    /// </summary>
    public class ConsumeObjectiveInstance : ObjectiveInstance
    {
        private readonly QuestObjectiveConsume _config;
        private readonly Action<InventoryChangeInfo<long>> _onInventoryChange;

        public ConsumeObjectiveInstance(QuestObjectiveConsume config) : base(config)
        {
            _config = config;
            _onInventoryChange = OnInventoryChange;
        }

        protected override void SubscribeEvents()
        {
            EventManager.Add(Inventory.EventInventoryChange, _config.ItemId, _onInventoryChange);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(Inventory.EventInventoryChange, _config.ItemId, _onInventoryChange);
        }

        private void OnInventoryChange(InventoryChangeInfo<long> changeInfo)
        {
            if (changeInfo.changeType != InventoryChangeType.Remove) return;

            // Remove operations report a negative amount; consuming increases progress by the magnitude.
            IncreaseProgress((int)-changeInfo.amount);
        }
    }
}
