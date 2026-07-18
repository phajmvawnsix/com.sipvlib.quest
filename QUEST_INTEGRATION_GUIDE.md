# Quest — Integration Guide

## Setup

`SiPV.Quest.asmdef` references `SiPV.Event`, `SiPV.Config`, `SiPV.UserData`, `SiPV.Shop`,
`SiPV.Ads`, `SiPV.AntiCheat`, `SiPV.Utilities`, `SiPV.Debugging`, UniTask, and Odin Inspector.

Call `QuestManager.Instance.Initialize()` once at startup, after `ConfigManager` and
`UserDataManager` are available:

```csharp
var ok = await QuestManager.Instance.Initialize();
if (!ok) { /* ConfigManager or UserDataManager not ready — see CustomLog output */ }
```

## Persistence API

`QuestManager` persists through `UserDataManager`, one key per quest id:

```csharp
// Save (called automatically on state change if _autoSaveOnChange is true)
await UserDataManager.Instance.SetAsync(questId, questSaveData); // QuestSaveData

// Load (called once during Initialize())
var all = await UserDataManager.Instance.GetAllAsync<QuestSaveData>();
```

There is no `Load`/`Save` string-key API on `QuestSaveData` — always go through
`SetAsync`/`GetAllAsync<QuestSaveData>`.

## Objective integration contracts

What game code must call for each objective type to progress:

- **Consume / Receive** — nothing Quest-specific. Call
  `Inventory.Remove(itemId, amount, source)` / `Inventory.Add(itemId, amount, source)` as
  normal; the objective observes `Inventory.EventInventoryChange` automatically.
- **EnemyKill** — call
  `EventManager.Invoke(QuestEvents.EventEnemyKilled, new QuestEvents.EnemyKilledEvent { enemyTypeId = ..., amount = 1 });`
  when an enemy dies. This is the one event Quest originates itself — no SiPVLib module
  tracks combat kills.
- **WatchAds** — nothing Quest-specific. A normal `AdsManager.ShowRewardedAd(...)` call that
  grants a reward fires `AdsManager.EventAdsRewarded`, which the objective observes directly.
- **BuyShopItem** — nothing Quest-specific. A normal `ShopManager.Instance.PurchaseItem(...)`
  call that succeeds fires `ShopManager<T>.EventShopResult`, which the objective observes
  directly (subscribed globally and filtered by item id, since the per-item target key is
  private to `ShopManager<T>`).
- **VisitFeature** — call `UserDataManager.Instance.IncrementFeatureAmountCounter(featureId)`
  at the moment of the visit (e.g. opening a shop screen). Quest does not detect visits on
  its own — it only observes the counter changing.
- **LoginOnDay / LoginStreak** — call
  `UserDataManager.Instance.IncrementFeatureAmountCounter(loginFeatureId)` (default id
  `"login"`, `QuestLoginKeys.DefaultFeatureId`) once per calendar day at app-start/login.
  `FeatureAmountCounter` does not enforce once-per-day itself — that's the caller's
  responsibility.

## Login/streak/visit-feature limitations

- **LoginStreak counts cumulative distinct login-increments, not a true consecutive-day
  streak.** No SiPVLib module tracks streak-with-reset-on-miss (`FeatureAmountCounter` has no
  such field), and Quest deliberately does not invent new persistence for it. A player who
  logs in on days 1, 2, and 5 gets progress `3`, the same as one who logs in on days 1, 2, 3 —
  there is no break detection.
- **VisitFeature / LoginOnDay / LoginStreak all rely on the caller incrementing the right
  counter at the right time.** Quest is a passive observer of `UserDataManager`'s save events;
  it cannot detect a visit or a login by itself.
- **BuyShopItem / VisitFeature / LoginStreak use a baseline snapshot taken when the quest is
  accepted** (`ObjectiveInstance.CaptureBaseline`), so pre-existing purchases/visits/logins
  before quest acceptance are not retroactively credited.

## Migration notes (breaking change)

`Events/QuestEventBus.cs` has been removed. Any code calling
`QuestEventBus.BroadcastItemCollected/Consumed/EnemyKilled/AdsWatched` must be updated:

- `BroadcastItemCollected`/`BroadcastItemConsumed` → call `Inventory.Add`/`Inventory.Remove`
  directly; nothing else to call.
- `BroadcastAdsWatched` → call `AdsManager.ShowRewardedAd(...)` as normal; nothing else to call.
- `BroadcastEnemyKilled` → replace with
  `EventManager.Invoke(QuestEvents.EventEnemyKilled, new QuestEvents.EnemyKilledEvent { ... });`.
