# Commercial & License-Based Roadmap

To transition RaceOverlay from a technical proof-of-concept to a commercial product that can compete with and beat RaceLabs, we need to focus on two pillars: **Commercial Infrastructure** (to support licenses) and **Premium Value-Adds** (to justify the price).

## 1. Core Commercial Infrastructure
These features are mandatory for any license-based software to manage users, prevent piracy, and provide a professional experience.

### Licensing & User Management
- **User Accounts (SSO):** Integration with iRacing, Discord, or Google for easy login.
- **License Key System:** Backend service to validate active subscriptions/one-time licenses.
- **Hardware ID (HWID) Locking:** Prevent sharing of a single license across multiple PCs.
- **Offline Mode:** Allow using the overlay for a limited time (e.g., 24h) without an internet connection if a valid license was recently verified.

### Cloud & Data Sync
- **Profile Cloud Sync:** Save widget layouts, custom themes, and per-car settings to the cloud so they follow the user.
- **Telemetry History:** Store historical lap data in the cloud for later analysis (RaceLabs offers this in their higher tiers).
- **Global Leaderboards:** Compare your best laps against other RaceOverlay users in real-time.

### Deployment & Maintenance
- **Silent Auto-Updates:** Using Velopack (already scaffolded in `App.xaml.cs`) to ensure users are always on the latest version.
- **Crash Reporting & Analytics:** Integrated Sentry/AppCenter to proactively fix bugs for paying customers.

---

## 2. Premium Value-Adds (The "RaceLabs Killers")
To beat RaceLabs, we shouldn't just copy them; we need features they lack or don't do well.

### AI & Strategy (The "Virtual Race Engineer")
- **Dynamic Fuel Strategy:** Instead of just "fuel to add", provide "save fuel" vs "push" scenarios based on current pace and traffic.
- **Tire Life Prediction:** Predict when tires will hit the "cliff" based on historical wear data and track temperature.
- **Traffic Awareness:** A widget that calculates where you will exit the pits relative to traffic (PIT Window overlay).

### Advanced Driver Development
- **Ghost Lap Overlay:** A real-time delta bar that compares your current lap against your personal best or a professional's telemetry trace (side-by-side trace).
- **Brake Shape Analysis:** A widget that analyzes your trail-braking and provides a "score" or visual feedback on your brake release vs a target shape.
- **Sector Analysis:** Real-time feedback on which specific corners you are losing time in compared to your optimal lap.

### Streamer & Creator Tools
- **Custom Branding:** Allow streamers to upload their own logos/colors to be integrated into the widgets.
- **Web-Based "Broadcast" Overlays:** A way for race broadcasters to view the telemetry of multiple drivers via a web link (requires a backend relay).
- **Twitch Integration:** Show "Sub Goals" or "Last Follower" as an overlay widget so streamers don't need OBS to see their stats.

---

## 3. Tiered Feature Model (Example)

| Feature | Free / Basic | Pro (Licensed) |
|---------|:---:|:---:|
| Core Widgets (Relative, Fuel, Inputs) | ✅ | ✅ |
| Custom Layouts | 1 | Unlimited |
| iRacing Support | ✅ | ✅ |
| Cloud Sync | ❌ | ✅ |
| AI Race Engineer | ❌ | ✅ |
| Historical Data Analysis | ❌ | ✅ |
| Custom Branding/Theming | ❌ | ✅ |

---

## 4. Implementation Priorities for Licensing
1. **Authentication Layer:** Implement a login screen on app startup.
2. **Feature Flags:** Modify `IWidgetRegistry` to filter widgets based on the user's license level.
3. **Backend API:** Create a minimal API (FastAPI or ASP.NET Core) to handle license checks and telemetry storage.
