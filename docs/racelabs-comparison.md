# RaceOverlay vs. RaceLabs: Widget Gap Analysis

This document provides a detailed comparison between the currently implemented widgets in RaceOverlay and those offered by RaceLabs. It identifies missing features and provides specifications for new widgets to achieve parity and eventually exceed the competitor's offering.

**Last Updated:** 2026-02-15

## 1. Feature Comparison Matrix

| Widget Category | RaceLabs Widget | RaceOverlay Status | Gap / Notes |
| :--- | :--- | :--- | :--- |
| **Leaderboards** | Standings, Relative | **Implemented** | Standings has pit status indicator and class colors. Missing: "Class Only" filter to hide other classes. |
| **Telemetry** | Inputs, Input Trace | **Implemented** | Inputs (throttle/brake/clutch/steering/gear/speed) + scrolling Input Trace graph. Color customization supported. |
| **Fuel & Strategy** | Fuel Calculator | **Implemented** | Fuel remaining, per-lap consumption, laps remaining, fuel needed to finish, and refill amount for pit stop. |
| **Navigation** | Track Map | **Implemented** | Auto-recorded track outlines with caching, class-colored car dots, pit status visualization. Missing: zoom & rotation. |
| **Environment** | Weather | **Implemented** | Current conditions, temps, humidity, wind (speed + 16-point compass direction), rain probability, forecast. |
| **Timing** | Lap Timer | **Implemented** | Current/last/best lap times, delta to best, out-lap detection. |
| **Safety** | Radar | **Implemented** | Top-down proximity radar (40m range), class-colored car rectangles, configurable colors. Missing: sound alerts, blind spot indicators. |
| **Head-to-Head** | Battle / Data | **Partial** | Relative Overlay shows drivers ahead/behind with gap deltas and Elo grades. Missing: dedicated 1v1 battle view with lap history and sector analysis. |
| **Driver Info** | Driver Profile / Elo | **Partial** | Relative Overlay shows Elo grade badges (A+ to R). Standings shows iRating and license class. Missing: dedicated driver profile card. |
| **Pit Info** | Pit Wall | **MISSING** | No dedicated pit stop timing, pit lane speed warning, or service monitoring widget. |
| **Social/Stream** | Chat / Recent Follows | **MISSING** | No Twitch/YouTube integration. |

---

## 2. Currently Implemented Widgets (9 Total)

| # | Widget | Update Rate | Key Capabilities |
| :--- | :--- | :--- | :--- |
| 1 | **Standings** | 500ms | Full leaderboard, positions gained/lost, iRating, license, interval, gap, last lap, delta, pit status |
| 2 | **Relative Overlay** | 500ms | Configurable drivers ahead/behind, gap delta, Elo grades (A+/A/B/C/D/R), stint info, class colors |
| 3 | **Radar** | 33ms (30Hz) | Top-down proximity view, 40m range, lateral + longitudinal offsets, class colors |
| 4 | **Track Map** | 50ms | Auto-recorded outlines, cached per track, class-colored dots, pit status opacity |
| 5 | **Lap Timer** | 50ms | Current/last/best lap, delta to best, lap counter, out-lap detection |
| 6 | **Fuel Calculator** | 1000ms | Fuel remaining (L + %), per-lap consumption, laps remaining, fuel to finish, refill amount |
| 7 | **Inputs** | 16ms (60Hz) | Steering rotation, throttle/brake/clutch bars, gear, speed |
| 8 | **Input Trace** | 16ms (60Hz) | Scrolling graph (configurable history), throttle + brake lines |
| 9 | **Weather** | 2000ms | Conditions, air/track temp, humidity, wind speed/direction, rain chance, forecast |

---

## 3. Remaining Gaps (Priority Order)

### Priority 1: Missing Features on Existing Widgets

#### A. Radar: Sound Alerts & Blind Spot Indicators
*   **Current State:** Visual-only proximity radar.
*   **Missing:**
    *   Audio alerts for "Car Left / Car Right" proximity warnings.
    *   Blind spot indicator overlays (colored bars on screen edges).
    *   Dynamic color transitions (Green -> Yellow -> Red) based on distance.
*   **Impact:** High - audio cues are critical when the driver's eyes are on the road.

#### B. Standings: Class-Only Filter
*   **Current State:** Shows all drivers with class color stripe and pit status indicator.
*   **Missing:**
    *   Toggle to filter and show only the player's class.
    *   Highlight/dim drivers currently in pits (beyond the "PIT" label).
*   **Impact:** Medium - essential for multi-class races (IMSA, Daytona 24).

#### C. Track Map: Zoom & Rotation
*   **Current State:** Static top-down view with auto-recorded outlines.
*   **Missing:**
    *   Auto-rotate based on car heading.
    *   Auto-zoom in congested areas (start/finish, chicanes).
    *   Manual zoom controls.
*   **Impact:** Medium - improves spatial awareness in tight racing.

#### D. Head-to-Head: Dedicated Battle View
*   **Current State:** Relative Overlay shows gaps and Elo grades for nearby drivers.
*   **Missing:**
    *   Focused 1v1 comparison panel (player vs. selected opponent).
    *   Last 5 laps side-by-side comparison.
    *   Sector-by-sector strengths/weaknesses analysis.
    *   Gaining/losing trend indicator.
*   **Impact:** Medium - enhances competitive awareness.

### Priority 2: New Widgets

#### A. Pit Wall Widget
*   **Description:** Dedicated display for pit lane activity and strategy.
*   **Key Features:**
    *   Pit window calculator (estimated laps until pit based on fuel consumption).
    *   Pit lane speed limit warning with countdown.
    *   Service timer (time spent on tires/fuel during stop).
    *   Undercut/overcut gap analysis.
*   **Impact:** High for endurance races.

### Priority 3: Nice-to-Have

#### A. Driver Profile Card
*   **Description:** Pop-up detailed stats about a specific driver on track.
*   **Key Features:**
    *   Full iRating history, license class, safety rating.
    *   Recent race results.
    *   Car/class information.
*   **Note:** Partially covered by Relative Overlay (Elo grades) and Standings (iRating + license).

#### B. Social/Streamer Integration
*   **Description:** Twitch/YouTube chat overlay and follower alerts.
*   **Impact:** Low priority for core racing functionality, high for streamer market.

---

## 4. Updated Implementation Roadmap

1.  **Phase 1 (Polish Existing):** Add **sound alerts** to Radar, **class-only filter** to Standings, and **zoom/rotation** to Track Map. These are incremental improvements to already-working widgets.
2.  **Phase 2 (Strategy):** Build the **Pit Wall Widget** for endurance race support.
3.  **Phase 3 (Competition):** Build dedicated **Head-to-Head Battle Widget** with lap history and sector analysis.
4.  **Phase 4 (Customization):** Add **theme/color/font customization** system across all widgets for streamer appeal.
5.  **Phase 5 (Social):** Add **Twitch/YouTube integration** if targeting the streamer market.
