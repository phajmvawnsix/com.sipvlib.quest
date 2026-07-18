# com.sipvlib.quest

Part of [SiPVLib](https://github.com/phajmvawnsix/SiPVLib). A quest/mission tracking system (`QuestManager`/`QuestInstance`/`ObjectiveInstance`) with pluggable objective types (consume, receive, watch ads, buy shop item, enemy kill, login streak, login on day, visit feature), reward granting, and event broadcasting.

## Install

Add to your project's `Packages/manifest.json`:

```json
"com.sipvlib.quest": "https://github.com/phajmvawnsix/com.sipvlib.quest.git",
"com.sipvlib.ads": "https://github.com/phajmvawnsix/com.sipvlib.ads.git",
"com.sipvlib.anticheat": "https://github.com/phajmvawnsix/com.sipvlib.anticheat.git",
"com.sipvlib.config": "https://github.com/phajmvawnsix/com.sipvlib.config.git",
"com.sipvlib.debugging": "https://github.com/phajmvawnsix/com.sipvlib.debugging.git",
"com.sipvlib.event": "https://github.com/phajmvawnsix/com.sipvlib.event.git",
"com.sipvlib.shop": "https://github.com/phajmvawnsix/com.sipvlib.shop.git",
"com.sipvlib.userdata": "https://github.com/phajmvawnsix/com.sipvlib.userdata.git",
"com.sipvlib.utilities": "https://github.com/phajmvawnsix/com.sipvlib.utilities.git",
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
"com.gameworkstore.googleprotobufunity": "https://github.com/GameWorkstore/google-protobuf-unity.git#3.15.2012"
```

UPM does not automatically resolve nested git dependencies — you must add the `com.sipvlib.*`, UniTask, and protobuf entries above yourself alongside this package.

## Optional: Odin Inspector

This package integrates with [Odin Inspector](https://odininspector.com) (Sirenix) if you have it installed, but does NOT require it and does NOT bundle it — Odin is a paid Unity Asset Store asset and cannot be redistributed here.

- **Without Odin installed**: `QuestObjective` (and its subclasses) work fully with plain Unity Inspector rendering — the `_requiredAmount` field renders as a normal int field, without the clamped range slider.
- **With Odin installed** (purchase + import from the Asset Store, which auto-defines the `ODIN_INSPECTOR` scripting define symbol): `QuestObjective` lights up Odin's `PropertyRange` attribute, clamping `_requiredAmount` to a 1–10000 slider in the inspector.

No manual setup is needed beyond installing Odin itself — detection is automatic via the `ODIN_INSPECTOR` define.

## Documentation
- [Quest integration guide](QUEST_INTEGRATION_GUIDE.md)
- [Usage guide](USAGE.md) — original module documentation carried over from the SiPVLib monolith
