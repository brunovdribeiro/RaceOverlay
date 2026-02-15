# RaceOverlay – Technical Architecture
## SaaS + Desktop Integration

---

# 1. High-Level Overview

RaceOverlay consists of two main systems:

1. Desktop Application (WPF / .NET)
2. Cloud SaaS Platform (Web + API + Infrastructure)

The desktop application provides real-time overlays.
The SaaS platform provides licensing, authentication, track synchronization,
updates, analytics, and future AI capabilities.

---

# 2. System Architecture Diagram (Logical)

[ Desktop App ]
|
| HTTPS (REST + JWT)
v
[ API Gateway / Backend API ]
|
|------------------------------|
|              |              |
Auth Service   License Service   Track Service
|              |              |
|              |              |
User DB       License DB       Track DB + Storage
|
v
Payment Provider (Stripe)

Static:
- Website (Marketing + Dashboard)
- CDN (Installer + Track Maps)

---

# 3. Desktop Application Architecture

## 3.1 Core Responsibilities

- Telemetry ingestion
- Overlay rendering
- Widget management
- Track map capture
- Authentication
- License validation
- Update client
- Log generation
- Secure API communication

## 3.2 Desktop → SaaS Communication

All communication must use:
- HTTPS only
- JWT authentication
- Retry policy
- Circuit breaker pattern
- Timeout handling

## 3.3 Authentication Flow

First Launch:
1. User logs in (email + password)
2. Desktop calls `/auth/login`
3. Server returns:
    - Access token (short-lived)
    - Refresh token
4. Tokens stored securely (Windows DPAPI)

Subsequent Launch:
- Validate token
- Refresh if expired

Offline Mode:
- Cached license allowed
- Grace period (e.g., 7 days)
- Encrypted local validation record

---

# 4. Backend SaaS Architecture

## 4.1 Core Services

### 4.1.1 Auth Service
- User registration
- Email verification
- Password reset
- JWT issuing
- Role management

### 4.1.2 License Service
- Subscription validation
- Device activation tracking
- Plan enforcement
- Grace period handling

### 4.1.3 Track Map Service
- Upload endpoint
- Validation
- Deduplication
- Versioning
- Distribution

### 4.1.4 Version Service
- App version metadata
- Release notes
- Update channel (stable/beta)

---

# 5. API Design (REST)

## Auth
POST   /auth/register
POST   /auth/login
POST   /auth/refresh
POST   /auth/logout

## License
GET    /license/status
POST   /license/activate
POST   /license/deactivate

## Track Maps
GET    /tracks/{game}/{trackId}
POST   /tracks/upload

## Updates
GET    /versions/latest?channel=stable

---

# 6. Database Design (Simplified)

## Users
- Id (GUID)
- Email
- PasswordHash
- CreatedAt
- IsVerified
- Role

## Subscriptions
- Id
- UserId
- Plan
- Status
- StripeCustomerId
- StripeSubscriptionId
- RenewalDate

## Licenses
- Id
- UserId
- PlanType
- ActiveDevices
- MaxDevices
- LastValidation

## TrackMaps
- Id
- Game
- TrackId
- Version
- UploadedBy
- CreatedAt
- StoragePath

---

# 7. Payment Integration (Stripe)

Flow:

1. User selects plan on website
2. Stripe Checkout
3. Stripe webhook → Backend
4. Backend:
    - Creates subscription record
    - Activates license
5. Desktop validation becomes valid

Must implement:
- Webhook signature validation
- Failed payment handling
- Automatic suspension logic

---

# 8. Track Map Sync Flow

## Unknown Track Flow:

1. Desktop detects unknown track
2. Captures first valid lap coordinates
3. Normalizes data
4. Upload to `/tracks/upload`
5. Backend:
    - Validates shape consistency
    - Checks duplicates
    - Stores new version
6. Other users download automatically

Caching:
- Local cache
- Version check before download

---

# 9. Auto Update System

Flow:

1. Desktop calls `/versions/latest`
2. If version > current:
    - Download from CDN
    - Verify signature
    - Replace binary
    - Restart app

Security:
- Installer must be signed
- Update file must include hash verification

---

# 10. Infrastructure Recommendation

Cloud Provider:
- AWS / Azure

Components:
- API (containerized)
- Managed database (PostgreSQL)
- Object storage (S3 / Blob)
- CDN (CloudFront / Azure CDN)
- Stripe integration
- Monitoring (CloudWatch / App Insights)

CI/CD:
- GitHub Actions
- Automated build
- Automated tests
- Signed artifacts
- Deployment pipeline

---

# 11. Security Considerations

- HTTPS only
- JWT with expiration
- Refresh token rotation
- Password hashing (Argon2/Bcrypt)
- Rate limiting
- Input validation
- SQL injection protection
- XSS protection (website)
- Secure local storage (DPAPI)

---

# 12. Observability

- Structured logging
- Centralized logs
- Metrics:
    - License validations
    - API errors
    - Track uploads
    - Crash reports
- Alerting for:
    - API downtime
    - Payment failures
    - High error rates

---

# 13. Scalability Strategy

Phase 1:
- Single API instance
- Managed DB

Phase 2:
- Horizontal scaling
- Load balancer
- Caching layer (Redis)
- AI processing workers

---

# 14. Future AI Layer Integration

Future services:

- Telemetry upload endpoint
- Session processing pipeline
- ML model inference service
- AI Tutor voice generation
- Driver performance scoring

Architecture must remain modular to allow AI services without rewriting core backend.
