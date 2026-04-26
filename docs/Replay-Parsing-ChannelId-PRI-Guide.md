# Replay Parsing Guide: `ChannelId`, `PRI`, and Player Name Resolution

## Purpose

This document explains how `ChannelId` and `PRI` values relate to each other in Rocket League replay parsing, and how to correctly map:

1. Car actor -> player PRI
2. Player PRI -> player name
3. Boost pickup -> player name

It also explains why `"Unknown"` appears and how to debug it quickly.

---

## Core Concepts

### 1) `ChannelId` is actor-specific

`ChannelId` is not one global player ID. Its meaning depends on `ObjectName`.

- For `TAGame.Car_TA`, `ChannelId` is the **car actor channel**.
- For `TAGame.PRI_TA`, `ChannelId` is the **PRI actor channel** (player identity actor).
- For `TAGame.VehiclePickup_Boost_TA`, `ChannelId` is the **boost pad actor channel**.

---

### 2) What is `PRI`?

`PRI` (PlayerReplicationInfo) is the actor that represents player identity/state in replay data.

In `Car_TA` updates, this usually appears as:

- `ActorData.PlayerReplicationInfo.TargetIndex`

That `TargetIndex` points to the player’s PRI channel/index.

---

## The Two Critical Maps

In `ReplayParseContext`:

- `ActiveCarToPlayerMap`  
  **Key:** car channel id  
  **Value:** player PRI index  
  Example: `126 -> 36`

- `ActiveCarToPlayerName`  
  **Key:** player PRI index  
  **Value:** player name  
  Example: `36 -> "vettel"`

These maps should not mix key types.

---

## Expected Data Flow

## Step 1: Car update arrives (`TAGame.Car_TA`)

From `CarUpdateHandler`:

- Read `carChannelId` from `update["ChannelId"]`
- Read `targetIndex` from `ActorData.PlayerReplicationInfo.TargetIndex`
- Store: `ActiveCarToPlayerMap[carChannelId] = targetIndex`

Example:
- car = `126`
- target PRI = `36`
- map now: `126 -> 36`

---

## Step 2: PRI update arrives (`TAGame.PRI_TA`)

From `PriUpdateHandler`:

- Read `priChannelId` from `update["ChannelId"]`
- Read `playerName` from `ActorData.PlayerName`
- Store: `ActiveCarToPlayerName[priChannelId] = playerName`

Optional validation:
- ensure `priChannelId` exists in `ActiveCarToPlayerMap.Values`

Example:
- PRI channel = `36`
- name = `vettel`
- map now: `36 -> "vettel"`

---

## Step 3: Boost update arrives (`TAGame.VehiclePickup_Boost_TA`)

From `BoostUpdateHandler`:

- Read `instigatorActorId` (usually car channel id)
- Lookup PRI: `ActiveCarToPlayerMap[instigatorActorId] => playerPriIndex`
- Lookup name: `ActiveCarToPlayerName[playerPriIndex] => playerName`

Example:
- instigator car = `126`
- `126 -> 36`
- `36 -> "vettel"`
- result: `vettel grabbed boost`

---

## Why `"Unknown"` happens

Most common causes:

1. **Wrong key type in name map**
   - storing by car channel, reading by PRI index (or vice versa)

2. **Wrong lookup direction**
   - using `TryGetValue` on car->PRI map with a PRI id key

3. **Event ordering**
   - boost event occurs before PRI name mapping appears

4. **Missing/empty `ActorData.PlayerName` in that frame**
   - name not available yet

---

## Practical Rule of Thumb

- `Car_TA.ChannelId` => car key
- `PRI_TA.ChannelId` => PRI key
- `PlayerReplicationInfo.TargetIndex` => points to PRI key
- Name dictionary should be keyed by PRI key