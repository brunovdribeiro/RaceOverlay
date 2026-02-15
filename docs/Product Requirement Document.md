# RaceOverlay – Product Requirements Document (PRD)

Version: 1.0  
Status: Draft  
Owner: Product & Engineering

---

# 1. Product Overview

RaceOverlay is a high-performance overlay system for sim racing games,
designed to compete directly with RaceLabs in the overlay space.

Phase 1 focuses on overlay functionality with SaaS-backed licensing
and track synchronization.

Future phases will introduce AI-powered telemetry analysis and driver coaching.

---

# 2. Goals

## 2.1 Business Goals

- Launch competitive overlay platform
- Achieve feature parity with RaceLabs core overlays
- Enable monetization via subscription
- Build SaaS foundation for AI expansion

## 2.2 Product Goals

- Minimal FPS impact
- Stable telemetry ingestion
- Seamless auto-detection
- Frictionless onboarding
- Reliable license validation

---

# 3. Target Users

Primary:
- Competitive sim racers (iRacing, rFactor 2, LMU)

Secondary:
- Streamers
- League racers
- Data-focused drivers

---

# 4. Core Features (Phase 1)

## 4.1 Overlay System

- Relative Overlay
- Standings
- Lap Timer
- Fuel Calculator
- Radar
- Track Map
- Inputs & Trace
- Weather

Requirements:
- Drag & reposition
- Persistent layout
- Per-game profile
- Minimal latency

---

## 4.2 Desktop Requirements

- Windows 10/11
- .NET Runtime
- Auto-update system
- Secure authentication
- License validation
- Offline grace period
- Log export feature

---

## 4.3 SaaS Platform

- Website (marketing + dashboard)
- User registration
- Subscription management
- Payment integration (Stripe)
- Track map sync
- Version check API

---

# 5. Non-Functional Requirements

## Performance
- < 3% CPU usage target
- No noticeable FPS drop
- UI update latency < 50ms

## Reliability
- Graceful telemetry disconnect recovery
- Crash logging
- Automatic reconnection

## Security
- Secure license validation
- Encrypted local token storage
- HTTPS only communication

---

# 6. Monetization

Model: Subscription

Plans (example):
- Free Tier (limited widgets)
- Pro Tier (full overlays)
- Future AI Tier

Trial:
- 7–14 day free trial

---

# 7. Success Metrics

- Daily Active Users
- Conversion Rate (Free → Paid)
- Crash rate < 1%
- Track map coverage %
- Support ticket volume

---

# 8. Risks

- Telemetry inconsistencies
- FPS impact perception
- Payment disputes
- License abuse
- Competition response

---

# 9. Roadmap

Phase 1:
- Overlay release
- SaaS licensing
- Track sync
- Auto-update

Phase 2:
- Per-track profiles
- Advanced widgets
- Multi-monitor improvements

Phase 3:
- Telemetry upload
- AI analysis dashboard
- AI tutor beta

---

# 10. Definition of Done (Launch)

- Installer signed
- Website live
- Subscription working
- License validation stable
- Auto-update functional
- Track sync working
- Crash logging implemented
- Documentation complete

---

# 11. Out of Scope (Phase 1)

- Real-time AI coaching
- Telemetry cloud analytics
- Mobile application
- Cross-platform support
