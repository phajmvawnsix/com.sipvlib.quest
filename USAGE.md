# Quest

Quest tracking, progress, and reward-claiming for SiPVLib games. `QuestManager` accepts/abandons
quests, evaluates unlock conditions, tracks objective progress, and persists state through
`UserDataManager`. It is a thin consumer of events other SiPVLib modules already broadcast —
it does not duplicate their tracking.

## Architecture

- `ConfigQuest` (`SiPVLib.Config.Configs.GameConfig`) — static quest data: title/description,
  `UnlockedConditions` (a `GameConditionGroup`), `ExecutionMode` (Sequential/Parallel),
  `Objectives` (`QuestObjective[]`), `Reward`.
- `QuestObjective` (abstract) — one config subclass per objective type (`Configs/QuestObjective*.cs`).
  `CreateInstance()` returns the matching runtime subclass.
- `QuestInstance` — runtime state machine for an accepted quest: `Inactive → Active →
  Completed | Failed | Abandoned`. Owns an `ObjectiveInstance[]` and fires `UnityEvent`s on
  state/objective changes.
- `ObjectiveInstance` (abstract, one subclass per type under `Runtime/ObjectiveInstances/`) —
  tracks `CurrentAmount`/`RequiredAmount`. `Subscribe()`/`Unsubscribe()` register/remove the
  exact `EventManager` listener(s) that drive its progress; `QuestManager` calls these
  symmetrically whenever a quest becomes active or stops being active, so abandoned/completed
  quests don't keep listening.
- `QuestManager` (`MonoSingleton`) — owns config lookup, active-quest tracking, unlock
  auto-accept/relock, and persistence.

## Event integration

| Objective type | SiPVLib event consumed | Producing module |
|---|---|---|
| Consume | `Inventory.EventInventoryChange` (Remove, targeted by item key) | `SiPVLib.UserData.InventoryHelper.Inventory` |
| Receive | `Inventory.EventInventoryChange` (Add, targeted by item key) | `SiPVLib.UserData.InventoryHelper.Inventory` |
| EnemyKill | `QuestEvents.EventEnemyKilled` | Quest itself — gameplay code must call this |
| WatchAds | `AdsManager.EventAdsRewarded` | `SiPVLib.Ads.AdsManager` |
| BuyShopItem | `ShopManager{T}.EventShopResult` | `SiPVLib.Shop.ShopManager<T>` |
| VisitFeature | `UserDataManager.EventUserDataSave` (pull `FeatureAmountCounter`) | `SiPVLib.UserData.UserDataManager` |
| LoginOnDay | `UserDataManager.EventUserDataSave` (matched against `GameTime.Now`) | `SiPVLib.UserData.UserDataManager` + `SiPVLib.AntiCheat.GameTime` |
| LoginStreak | `UserDataManager.EventUserDataSave` (pull `FeatureAmountCounter`) | `SiPVLib.UserData.UserDataManager` |

Quest never runs its own parallel pub/sub system — it always subscribes through
`SiPVLib.Event.EventManager` directly against the module that already owns the data.

## Public API (`IQuestManager`)

- `UniTask<bool> Initialize()`
- `bool IsInitialized { get; }`
- `QuestInstance AcceptQuest(string questId)`
- `void AbandonQuest(string questId)`
- `QuestInstance GetActiveQuest(string questId)`
- `void CheckQuestCompletion(string questId)` — force a completion/sequencing re-check
  without a fresh objective-completed event (e.g. after a save-game import).
- `bool ClaimQuestReward(string questId)`
- `QuestInstance[] GetActiveQuests()`
- `UniTask<bool> SaveQuestsAsync()`
- `QuestState GetQuestState(string questId)`

## Objective types

| Type | Config fields | Progress trigger | Push / pull |
|---|---|---|---|
| Consume | `ItemId` | Inventory item removed | Push |
| Receive | `ItemId` | Inventory item added | Push |
| EnemyKill | `EnemyTypeId` (optional filter) | `QuestEvents.EventEnemyKilled` | Push |
| WatchAds | — | Rewarded ad grants its reward | Push |
| BuyShopItem | `ItemId` | Successful shop purchase of that item | Push |
| VisitFeature | `FeatureId` | UserData feature counter increments | Pull (baseline delta) |
| LoginOnDay | `LoginFeatureId`, `Mode`, `TargetDayValue` | Login on matching day | Push |
| LoginStreak | `LoginFeatureId` | Login feature counter increments (cumulative, **not** consecutive-day streak — see limitation below) | Pull (baseline delta) |

## Persistence

`QuestManager.SaveQuestsAsync()` writes one `QuestSaveData` (quest id, state, per-objective
progress array, reward-claimed flag) per active quest via
`UserDataManager.Instance.SetAsync(questId, questSaveData)`. On `Initialize()`,
`UserDataManager.Instance.GetAllAsync<QuestSaveData>()` restores every saved quest, replays
`LoadState`, and re-subscribes objectives that are still `Active`.

## Extending

To add a new objective type:
1. Add a `Configs/QuestObjectiveXxx.cs` subclass of `QuestObjective` with its type-specific
   fields, overriding `CreateInstance()` to return a new `Runtime/ObjectiveInstances/XxxObjectiveInstance`.
2. Add the `ObjectiveInstance` subclass, overriding `SubscribeEvents()`/`UnsubscribeEvents()`
   to register/remove an `EventManager` listener against whichever module already owns the
   relevant state. Prefer reusing an existing module's event over inventing a new one.
3. For a pull-based (state-query) type, use `CaptureBaseline`/`ApplyProgressFromBaseline` so
   pre-existing progress in the source counter isn't retroactively credited.

See `QUEST_INTEGRATION_GUIDE.md` for what game code must call for each objective type to
actually progress.
