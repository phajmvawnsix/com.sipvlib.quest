using System;
using SiPVLib.Ads;
using SiPVLib.Ads.Events;
using SiPVLib.Event;
using SiPVLib.Quest.Configs;

namespace SiPVLib.Quest.Runtime.ObjectiveInstances
{
    /// <summary>
    /// Progress driven by <see cref="AdsManager.EventAdsRewarded"/> — a rewarded ad only fires
    /// this event once the reward is actually granted, so it's the safe "watched to completion"
    /// signal (as opposed to <c>EventAdsClose</c>, which fires even if the player skipped early).
    /// </summary>
    public class WatchAdsObjectiveInstance : ObjectiveInstance
    {
        private readonly Action<AdsEventRewarded> _onAdRewarded;

        public WatchAdsObjectiveInstance(QuestObjectiveWatchAds config) : base(config)
        {
            _onAdRewarded = OnAdRewarded;
        }

        protected override void SubscribeEvents()
        {
            EventManager.Add(AdsManager.EventAdsRewarded, _onAdRewarded);
        }

        protected override void UnsubscribeEvents()
        {
            EventManager.Remove(AdsManager.EventAdsRewarded, _onAdRewarded);
        }

        private void OnAdRewarded(AdsEventRewarded evt) => IncreaseProgress(1);
    }
}
