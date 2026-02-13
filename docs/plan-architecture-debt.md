# Plan: Architecture Debt Cleanup

## Current State

### WidgetOverlayWindow if/else Chain
`WidgetOverlayWindow.xaml.cs` has 3 methods each containing an 8-way if/else chain (one branch per widget type):
- `WidgetOverlayWindow_Loaded()` — creates view, viewmodel, applies config, subscribes events
- `RefreshWidgetData()` — calls refresh on the correct viewmodel
- `SubscribeDataUpdated()` / `UnsubscribeDataUpdated()` — event wiring

Total: ~24 if/else conditions. Adding a new widget requires editing 3+ methods.

### MainWindowViewModel Config Loading
`LoadConfigForWidget()` and `OnSelectedActiveCardChanged()` each have 8-way if/else chains to load/display widget-specific configuration.

## Goal
Replace type-checking if/else chains with a registration or factory pattern so new widgets can be added without modifying existing code.

## Approach: Widget Factory + Registration

### Steps

1. **Define `IWidgetViewFactory` interface** in Core
   ```csharp
   public interface IWidgetViewFactory
   {
       string WidgetId { get; }
       FrameworkElement CreateView();
       object CreateViewModel(IWidget widget, IWidgetConfiguration config);
   }
   ```

2. **Create a factory implementation per widget** in Engine
   - Each factory knows how to create its view, viewmodel, and wire up config
   - Register factories in a `Dictionary<string, IWidgetViewFactory>` at startup

3. **Refactor `WidgetOverlayWindow`**
   - Replace if/else chain with factory lookup: `_factories[widget.WidgetId].CreateView()`
   - Widget refresh/subscribe/unsubscribe become virtual methods on a base viewmodel

4. **Introduce `WidgetViewModelBase`** in Engine
   - Abstract base with `RefreshData()`, `SubscribeToUpdates()`, `UnsubscribeFromUpdates()`
   - Each widget viewmodel inherits from it
   - `WidgetOverlayWindow` calls base methods without type-checking

5. **Refactor `MainWindowViewModel` config loading**
   - Move config ↔ UI mapping into widget-specific config editor classes or use a generic approach
   - Replace if/else with dictionary lookup by widget ID

### Files to Create/Modify
| File | Action |
|------|--------|
| `src/RaceOverlay.Core/IWidgetViewFactory.cs` | Create |
| `src/RaceOverlay.Engine/ViewModels/WidgetViewModelBase.cs` | Create |
| `src/RaceOverlay.Engine/Factories/` (one per widget) | Create |
| `src/RaceOverlay.App/WidgetOverlayWindow.xaml.cs` | Modify — use factory |
| `src/RaceOverlay.App/ViewModels/MainWindowViewModel.cs` | Modify — use factory for config |
| `src/RaceOverlay.App/App.xaml.cs` | Modify — register factories |

### Acceptance Criteria
- [ ] No if/else type-checking chains remain in WidgetOverlayWindow
- [ ] No if/else type-checking chains remain in MainWindowViewModel
- [ ] Adding a new widget requires only creating new files, not modifying existing ones
- [ ] All 8 existing widgets work identically after refactor
- [ ] E2E tests pass
