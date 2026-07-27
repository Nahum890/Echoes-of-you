---
level: 4
name: "Anticipación"
archetype: Teaching
chapter: II
blueprint:
  echoEnabled: true
  maxEchoes: 1
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
  components:
    - { name: Plate_Echo_TimedA, type: PressurePlate, position: [0, 0, 22], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Platform_Timed,     type: MovingPlatform, position: [0, 0, 28], scale: [4, 0.3, 4], targetSignals: [Plate_Echo_TimedA], customData: "0,0,0|0,0,6|2.5|false" }
    - { name: Plate_Echo_TimedB, type: PressurePlate, position: [0, 0, 36], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Triggered,    type: Door, position: [0, 2, 40], scale: [4, 4, 0.4], targetSignals: [Plate_Echo_TimedB] }
    - { name: LevelExit_Area,     type: LevelExit, position: [0, 1.1, 56], customData: "Level_05" }
    - { name: LevelGoal,          type: LevelGoal, position: [0, 1.1, 56], customData: "Anticipa los 0.8s.|Llega exacto.|.", targetSignals: [Plate_Echo_TimedB] }
  wiring:
    - { src: Plate_Echo_TimedA, dst: Platform_Timed,       gate: AND }
    - { src: Plate_Echo_TimedB, dst: Door_Triggered,        gate: AND }
    - { src: Plate_Echo_TimedB, dst: LevelGoal,             gate: COUNT, requiredCount: 1 }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player starts at [0,0,-4] (PlayerStart of L04). Player walks the corridor to [0,0,16] — StandardPlatforms."
    - "Player records an Echo path: from [0,0,16] walk to [0,0,22] (start of Plate_Echo_TimedA), continuing forward toward [0,0,24]. Recording duration ~3 seconds. Then turns left and continues recording: walks to [0,0,36] (Plate_Echo_TimedB) — total recorded duration ~7 seconds (comfortable within 12s cap)."
    - "Player releases Record. Echo spawns at [0,0,16] at t=0."
    - "t=0.8s: Echo begins translating (latency). At t≈2s the Echo's body crosses onto Plate_Echo_TimedA at [0,0,22]. Plate_TimedA.IsPressed=true → TimedMovingPlatform.AtActiveLocal target moves. Platform_Timed begins its 2.5-second transit from [0,0,28] to [0,0,34] at 2.5 m/sec."
    - "Echo continues forward, exiting Plate_Echo_TimedA at t≈3s. The Echo's body residual releases Plate_Echo_TimedA — but Platform_Timed has been running its transit, which ends at t≈5.5s — well after the Echo cleared the platform zone."
    - "Echo reaches Plate_Echo_TimedB at t≈4s — before the platform reaches its max position (the Echo rides ON the platform's apex, in transit). Door_Triggered opens (latchOpen=false default — release closes)."
    - "Echo residual+residual keeps Door_Triggered open for at least 2.5s."
    - "Player must NOW physically traverse: walk onto Plate_TimedA at [0,0,22]? No, Echo has tagged it. Player cannot 'reopen' — Player must walk forward to where the platform will arrive at [0,0,34] at t≈5.5s, then ride the platform the reverse way? No — platform TIMED ONE-WAY ('fastReturn: false' means: return only when plate releases; once plate releases, it returns at multiplier of 1× since 'fastReturn=false' disabled; this would take 2.5s again)."
    - "IMPORTANT: The level is solved by the Echo riding Platform_Timed itself — but Echo's residual ends at t≈6.5s and the platform would still be outbound until Plate_TimedA releases (~3s) → Platform returns ~5.5s later. So actually the level is solved when Player walks onto Plate_Echo_TimedA themselves — but they can't because the plate is 'EchoOnly'. The level requires PLAYER to do nothing else but WATCH the Echo traverse from A → Platform_Timed → B until Door_Triggered opens, and Player THEMSELVES needs to walk through the door before residual expires."
    - "Player commits to: walk forward to [0,0,22.5], wait in front of closed door. When echo reaches Plate_TimedB (at t=4-6s, depending on platform timing), Door opens AND Player needs to immediately enter through and reach LevelExit at [0,1.1,56]."
---

# LEVEL 04 — Anticipación (Chapter II, opening)

**Archetype**: Teaching — first teaching of "ECHO_LATENCY = 0.8s" from ECHO_GRAMMAR §2.2 as a *design variable*, not as passive flavor. The player must learn that "Echo se demora."

## CustomData Convention for TimedMovingPlatform
The `customData` field at line 5 of the component:
```
"0,0,0|0,0,6|2.5|false"
```
Format: `inactiveLocal|activeLocal|travelSpeed|fastReturn` — pipe-separated Vector3|Vector3|float|bool.
- `inactiveLocal` = (0,0,0)
- `activeLocal` = (0,0,6) → 6 units forward
- `travelSpeed` = 2.5 m/s (transit duration = 6/2.5 = 2.4 s — close to but <2.5s residual)
- `fastReturn` = false (the return trip takes 2.4 s as well; not instant)

## Why 0.8s Latency is the Variable
The level-level learning moment: when the player records the Echo motion, the Echo's playback has the 0.8 s startup. So if you record 3 seconds of motion, the Echo is actually moving for 0.8 (latency) + 3 = 3.8 s = actual playback clock. If the player expects their recording to "press the plate by t=3s," they MUST understand the 0.8s + travel.

## Sync Floor
Not in play — only one Echo and one platform at a time. But timingFloor=0.4 is still set as project default — invalid scenarios are those where sync would require <0.4s.

## Echo Button Test
Backward BFS:
- LevelExit ← Door_Triggered ← Plate_Echo_TimedB (requiresEcho=true). PASS.

## Failure Modes
- Player records motion that DOESN'T reach TimedA within the platform's outbound transit window: platform starts; Echo keeps walking; platform stops because Echo leaves TimedA early; Echo then can't reach TimedB (no platform).
- Player mistimes when to release Record — Echo's recorded duration ends too early; Echo residual fires before platform arrives at TimedB; Door never opens.
- The 0.8 s latency is the prime teachable failure: if player records nothing and releases too early, Echo won't even start moving (length <0.2s threshold).
