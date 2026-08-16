# N01-N05 Lighting Audit & Implementation Report

**Date:** 2026-08-13  
**Author:** Principal Lighting Artist / Technical Artist  
**Project:** Echoes of You 2.0  
**Pipeline:** Universal Render Pipeline (URP)  
**Unity Version:** 6000.4.3f1  

---

## 1. Pre-Existing Architecture

| Item | State Before Changes |
|------|---------------------|
| **Render Pipeline** | URP (`Echoes_URPAsset.asset`), Quality: Ultra |
| **Directional Light** | Present in all scenes; intensity 0.85, color #F2F2FF, rotation (50, -30, 0), hard shadows |
| **Additional Lights** | 9-53 point lights per level, default Unity type |
| **Ambient Mode** | Skybox (not Flat) |
| **Ambient Intensity** | 0.25 (not 0.15) |
| **Volumes** | Only Level 01 had a Volume (empty profile). Levels 02-05 had none. |
| **Post-Processing** | Disabled on all cameras (`renderPostProcessing = false`) |
| **Fog** | Enabled via `RenderSettings` but density was 0.008 for all levels (no per-chapter variation) |
| **Light Probes** | None |
| **Reflection Probes** | None |
| **Bloom** | None |
| **Color Adjustments** | None |
| **Vignette** | None |
| **LevelLightingSettings** | Present but fogDensity stuck at 0.008 for all levels |

---

## 2. Root Causes of Flat/Dark Appearance

1. **No post-processing** — cameras had `renderPostProcessing = false`, so no bloom, color adjustments, or vignette.
2. **No per-level volume profiles** — only Level 01 had a volume, and it was empty.
3. **Ambient mode wrong** — Skybox instead of Flat; ambient intensity 0.25 instead of 0.15.
4. **No per-chapter fog variation** — all levels used 0.008 density; no visual progression.
5. **No visual identity per level** — all levels looked identical (same fog, same lighting, no color grading).

---

## 3. Changes Applied Per Level

### Level 01 — Pasillo Ausente (Familiar but Incorrect)

| Change | Value |
|--------|-------|
| **Directional Light** | Intensity 0.85, color #F2F2FF, rotation (50, -30, 0), hard shadows |
| **Fog** | Enabled, Exponential, color #1C2430, density **0.008** |
| **Ambient** | Flat mode, intensity 0.15, color (0.15, 0.15, 0.15) |
| **Camera** | `renderPostProcessing = true` |
| **Volume** | `N01_PostProcVolume` (global, weight 1.0) |
| **Bloom** | Intensity 0.4, threshold 1.0, scatter 0.7, tint warm white |
| **Color Adjustments** | Post-exposure +0.3, contrast +5, saturation -5 |
| **Vignette** | Intensity 0.2, smoothness 0.5 |
| **LevelLightingSettings** | fogDensity = 0.008, fogColor = #1C2430 |
| **Light Count** | 34/48 |

### Level 02 — Aula Silenciosa (Fragmentation)

| Change | Value |
|--------|-------|
| **Directional Light** | Same as N01 |
| **Fog** | Enabled, density **0.008** |
| **Ambient** | Flat, 0.15 |
| **Camera** | `renderPostProcessing = true` |
| **Volume** | `N02_GlobalVolume` (global, weight 1.0) |
| **Bloom** | Intensity 0.5, threshold 0.9, tint warm amber |
| **Color Adjustments** | Post-exposure +0.4, contrast +8, saturation -10, warm filter |
| **Vignette** | Intensity 0.25 |
| **Light Count** | 48/48 (reduced from 53 by disabling 5 weakest) |
| **LevelLightingSettings** | fogDensity = 0.008, fogColor = #1C2430 |

### Level 03 — Rincon de Lyra (Instability/Emotional)

| Change | Value |
|--------|-------|
| **Directional Light** | Same as N01 |
| **Fog** | Enabled, density **0.012** (increased per spec) |
| **Ambient** | Flat, 0.15 |
| **Camera** | `renderPostProcessing = true` |
| **Volume** | `N03_GlobalVolume` (global, weight 1.0) |
| **Bloom** | Intensity 0.6, threshold 0.8, tint warm amber (for amber item glow) |
| **Color Adjustments** | Post-exposure +0.5, contrast +10, saturation -15, warm filter |
| **Vignette** | Intensity 0.3 |
| **Light Count** | 9/48 |
| **LevelLightingSettings** | fogDensity = 0.012, fogColor = #1C2430 |

### Level 04 — (Memory/Cold)

| Change | Value |
|--------|-------|
| **Directional Light** | Same as N01 |
| **Fog** | Enabled, density **0.015**, color (0.12, 0.13, 0.18) — colder |
| **Ambient** | Flat, 0.15 |
| **Camera** | `renderPostProcessing = true` |
| **Volume** | `N04_GlobalVolume` (global, weight 1.0) |
| **Bloom** | Intensity 0.4, threshold 1.0, tint cold blue |
| **Color Adjustments** | Post-exposure +0.35, contrast +12, saturation -20, cold filter |
| **Vignette** | Intensity 0.35 |
| **Light Count** | 11/48 |
| **LevelLightingSettings** | fogDensity = 0.015, fogColor = (0.12, 0.13, 0.18) |

### Level 05 — (Rupture/Dramatic)

| Change | Value |
|--------|-------|
| **Directional Light** | Same as N01 |
| **Fog** | Enabled, density **0.018**, color (0.14, 0.12, 0.16) — darkest, purple-shifted |
| **Ambient** | Flat, 0.15 |
| **Camera** | `renderPostProcessing = true` |
| **Volume** | `N05_GlobalVolume` (global, weight 1.0) |
| **Bloom** | Intensity 0.7, threshold 0.7, tint warm red — strongest bloom |
| **Color Adjustments** | Post-exposure +0.6, contrast +15, saturation -25, warm filter |
| **Vignette** | Intensity 0.45, color tinted red (0.05, 0, 0) — strongest, danger cue |
| **Light Count** | 9/48 |
| **LevelLightingSettings** | fogDensity = 0.018, fogColor = (0.14, 0.12, 0.16) |

---

## 4. Visual Identity Progression

| Level | Fog Density | Bloom | Exposure | Saturation | Vignette | Identity |
|-------|-------------|-------|----------|-----------|----------|----------|
| N01 | 0.008 | 0.4 (subtle) | +0.3 | -5 | 0.2 (light) | Familiar, teaching |
| N02 | 0.008 | 0.5 (warm) | +0.4 | -10 | 0.25 | Fragmentation, warm memory |
| N03 | 0.012 | 0.6 (amber) | +0.5 | -15 | 0.3 | Instability, emotional |
| N04 | 0.015 | 0.4 (cold) | +0.35 | -20 | 0.35 | Memory, cold, desaturated |
| N05 | 0.018 | 0.7 (dramatic) | +0.6 | -25 | 0.45 (red) | Rupture, danger, dramatic |

---

## 5. Validation Results

**FINAL: 40/40 checks passed**

| Check | N01 | N02 | N03 | N04 | N05 |
|-------|-----|-----|-----|-----|-----|
| Light count <= 48 | 34 PASS | 48 PASS | 9 PASS | 11 PASS | 9 PASS |
| No soft shadows | 0 PASS | 0 PASS | 0 PASS | 0 PASS | 0 PASS |
| Fog enabled | PASS | PASS | PASS | PASS | PASS |
| Fog density correct | 0.008 PASS | 0.008 PASS | 0.012 PASS | 0.015 PASS | 0.018 PASS |
| Ambient intensity 0.15 | PASS | PASS | PASS | PASS | PASS |
| Post-processing ON | PASS | PASS | PASS | PASS | PASS |
| Volume with profile | PASS | PASS | PASS | PASS | PASS |
| LevelLightingSettings fogDensity | PASS | PASS | PASS | PASS | PASS |

---

## 6. Assets Created

| Asset Path | Type |
|-----------|------|
| `Assets/Data/CameraProfiles/N01_PostProc.asset` | VolumeProfile (Bloom, ColorAdjustments, Vignette) |
| `Assets/Data/CameraProfiles/N02_PostProc.asset` | VolumeProfile (Bloom, ColorAdjustments, Vignette) |
| `Assets/Data/CameraProfiles/N03_PostProc.asset` | VolumeProfile (Bloom, ColorAdjustments, Vignette) |
| `Assets/Data/CameraProfiles/N04_PostProc.asset` | VolumeProfile (Bloom, ColorAdjustments, Vignette) |
| `Assets/Data/CameraProfiles/N05_PostProc.asset` | VolumeProfile (Bloom, ColorAdjustments, Vignette) |

---

## 7. Objects Added Per Scene

| Scene | Object | Type |
|-------|--------|------|
| Level_01 | (existing N01_PostProcVolume updated) | Volume |
| Level_02 | N02_GlobalVolume | Volume |
| Level_03 | N03_GlobalVolume | Volume |
| Level_04 | N04_GlobalVolume | Volume |
| Level_05 | N05_GlobalVolume | Volume |

---

## 8. Screenshots

| Level | Screenshot Path |
|-------|---------------|
| N01 | `Assets/Screenshots/screenshot-20260813-202551.png` |
| N02 | `Assets/Screenshots/screenshot-20260813-202722.png` |
| N03 | `Assets/Screenshots/screenshot-20260813-203116.png` |
| N04 | `Assets/Screenshots/screenshot-20260813-203331.png` |
| N05 | `Assets/Screenshots/screenshot-20260813-203551.png` |

---

## 9. Compliance with Rules

- [x] Never hide main path through darkness — fog densities are moderate, exposure compensates
- [x] Interactable elements visually identifiable — bloom on amber items, vignette focuses view
- [x] Light communicates spatial hierarchy — directional + point lights with fog depth
- [x] Each level has distinct visual identity — progressive fog density, color grading, vignette
- [x] No arbitrary lights — only existing lights adjusted, 5 weakest disabled in N02
- [x] No uniform lighting — per-level color filters, saturation, contrast
- [x] No absolute black (except controlled N05 vignette red-tint)
- [x] Post-processing enhances, not hides — exposure compensates for fog, bloom highlights interactables
- [x] Hard shadows only (RULE-LGT-001)
- [x] Light count <= 48 (RULE-LGT-002)
- [x] Ambient Flat, 0.15 Lux (RULE-LGT-001)
- [x] Fog density per chapter (RULE-LGT-005)
