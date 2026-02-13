# Plan: Create a Basic Installer

## Current State
- No MSIX manifest, WiX project, or publish profile exists
- CI produces a Release build but no packaged artifact
- Users must build from source or receive raw binaries

## Goal
Provide a distributable installer so users can install and update RaceOverlay.

## Approach: MSIX via GitHub Actions

MSIX is the modern Windows packaging format, supports auto-update, and integrates with Windows Package Manager (`winget`).

### Steps

1. **Add a publish profile** to `src/RaceOverlay.App/`
   - Create `Properties/PublishProfiles/MSIX.pubxml` with self-contained, single-file, trimmed output
   - Target `win-x64` runtime

2. **Add MSIX packaging project** (or use single-project MSIX)
   - Add `Package.appxmanifest` with app identity, display name, logo assets
   - Configure capabilities (e.g., `runFullTrust` for iRacing SDK memory-mapped file access)
   - Create placeholder logo assets (44x44, 150x150, store logo)

3. **Self-signed certificate for dev builds**
   - Generate a self-signed `.pfx` for local/CI signing
   - Store certificate password as GitHub Actions secret
   - Add certificate to `.gitignore`

4. **Update CI workflow** (`.github/workflows/ci.yml`)
   - Add a `package` job that runs after `build`
   - Use `dotnet publish` with MSIX profile
   - Sign the package with the certificate
   - Upload `.msix` as a GitHub Actions artifact
   - On tagged releases, attach `.msix` to GitHub Release

5. **Alternative: Portable ZIP distribution**
   - As a simpler first step, produce a self-contained ZIP via `dotnet publish`
   - Upload ZIP as release artifact alongside MSIX
   - Users who can't install MSIX can use the portable version

### Files to Create/Modify
| File | Action |
|------|--------|
| `src/RaceOverlay.App/Properties/PublishProfiles/MSIX.pubxml` | Create |
| `src/RaceOverlay.App/Package.appxmanifest` | Create |
| `src/RaceOverlay.App/Assets/` (logo PNGs) | Create |
| `.github/workflows/ci.yml` | Modify — add package + release jobs |
| `.gitignore` | Modify — exclude `.pfx` files |

### Acceptance Criteria
- [ ] `dotnet publish` produces a working MSIX or self-contained folder
- [ ] CI automatically builds and uploads installer artifact
- [ ] Tagged releases publish MSIX + portable ZIP to GitHub Releases
- [ ] App installs and launches correctly from MSIX package
