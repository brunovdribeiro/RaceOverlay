# RaceOverlay vs. RaceLabs: Widget Gap Analysis

This document provides a detailed comparison between the currently implemented widgets in RaceOverlay and those offered by RaceLabs. It identifies missing features and provides specifications for new widgets to achieve parity and eventually exceed the competitor's offering.

## 1. Feature Comparison Matrix

| Widget Category | RaceLabs Widget | RaceOverlay Status | Gap / Notes |
| :--- | :--- | :--- | :--- |
| **Leaderboards** | Standings, Relative | **Implemented** | RaceLabs has advanced filtering (e.g., "In Pits", "Class Only"). |
| **Telemetry** | Inputs, Input Trace | **Implemented** | RaceOverlay has basic visualization; RaceLabs offers more styling options. |
| **Fuel & Strategy** | Fuel Calculator | **Implemented** | RaceOverlay covers basic fuel; missing "Refill amount" automation info. |
| **Navigation** | Track Map | **Implemented** | RaceLabs map is more polished with zooming features. |
| **Environment** | Weather | **Implemented** | Similar functionality. |
| **Timing** | Lap Timer | **Implemented** | Similar functionality. |
| **Safety** | **Radar** | **MISSING** | **Critical safety feature** for close-quarters racing. |
| **Head-to-Head** | Battle / Data | **MISSING** | Real-time delta comparison with a specific opponent. |
| **Pit Info** | Pit Wall | **MISSING** | Detailed pit stop timing and service monitoring. |
| **Social/Stream** | Chat / Recent Follows | **MISSING** | Integration for Twitch/YouTube streamers. |
| **Driver Info** | Driver Profile / Elo | **MISSING** | Detailed stats about specific drivers on track. |

---

## 2. Priority 1: Missing "Must-Have" Widgets

To be commercially viable, the following widgets should be implemented as soon as possible.

### A. Radar Widget
*   **Description:** A visual top-down or proximity indicator showing cars immediately to the left, right, or in the blind spots.
*   **Key Features:**
    *   Dynamic proximity bars (Green -> Yellow -> Red).
    *   Sound alerts for "Car Left / Car Right".
    *   Blind spot indicators.
*   **Technical Requirement:** Needs high-frequency XY coordinate data or "CarLeftRight" bitfield from telemetry.

### B. Head-to-Head (Battle) Widget
*   **Description:** Focused comparison between the player and the car immediately ahead or behind.
*   **Key Features:**
    *   Live delta (gaining/losing).
    *   Last 5 laps comparison.
    *   License/Rating of the opponent.
    *   Strengths/Weaknesses (e.g., "Faster in Sector 2").

### C. Pit Wall Widget
*   **Description:** Dedicated display for pit lane activity.
*   **Key Features:**
    *   Pit window calculator (estimated laps until pit).
    *   Pit lane speed limit warning.
    *   Count down to pit box.
    *   Service timer (time spent on tires/fuel).

---

## 3. Priority 2: Quality of Life & Polish

### A. Advanced Filtering for Standings
*   **Implementation:** Add toggles to the Standings widget to show only the current class, or highlight drivers currently in the pits.
*   **Value:** Reduces clutter in multi-class races (e.g., IMSA, Daytona 24).

### B. Map Zoom & Rotation
*   **Implementation:** Allow the Track Map to auto-rotate based on the car's heading and auto-zoom in congested areas (like the start/finish line).

---

## 4. Implementation Roadmap for Missing Widgets

1.  **Phase 1 (Safety):** Develop the **Radar Widget**. This is the most requested feature by users switching from other overlays.
2.  **Phase 2 (Competition):** Develop the **Head-to-Head Widget** to increase engagement during races.
3.  **Phase 3 (Strategy):** Enhance the **Fuel Calculator** and add the **Pit Wall** features.
4.  **Phase 4 (Streamer Support):** Add **Overlay Customization** (colors/fonts) to compete with RaceLabs' high-end visual polish.
