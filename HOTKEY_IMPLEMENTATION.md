# CTRL+F12 Hotkey Implementation for Widget Dragging

## Overview
Implemented a hotkey system that allows users to toggle all open overlay widgets into a draggable mode by pressing **CTRL+F12**. When enabled, widgets become clickable and draggable across the screen, allowing for interactive repositioning without closing them.

## Implementation Components

### 1. WidgetDragService (src/RaceOverlay.App/Services/WidgetDragService.cs)
**Purpose**: Singleton service that manages the dragging state across all open overlay windows.

**Key Features**:
- `IsDraggingEnabled` property - Indicates whether drag mode is currently active
- `RegisterWindow(WidgetOverlayWindow window)` - Registers a window for drag management
- `UnregisterWindow(WidgetOverlayWindow window)` - Unregisters a window when closed
- `ToggleDragging()` - Toggles drag mode for all registered windows
- `UpdateAllWindows()` - Propagates state changes to all windows

**Usage**:
```csharp
WidgetDragService.Instance.ToggleDragging(); // Toggle drag mode
WidgetDragService.Instance.IsDraggingEnabled  // Check current state
```

### 2. WidgetOverlayWindow (src/RaceOverlay.App/WidgetOverlayWindow.xaml.cs)
**Purpose**: Window that displays individual widgets with drag support.

**Key Enhancements**:
- `SetDraggingEnabled(bool enabled)` - Enables/disables drag mode for this window
- Mouse event handlers:
  - `MouseLeftButtonDown` - Initiates drag operation
  - `MouseMove` - Updates window position during drag
  - `MouseLeftButtonUp` - Completes drag operation
- Automatic cursor changes: Hand cursor when dragging enabled, Arrow when disabled
- Window registration with WidgetDragService on load and unregistration on close

**Drag Behavior**:
- Click and hold on the widget content area to drag
- Real-time window position updates as mouse moves
- Mouse capture prevents dragging outside window bounds

### 3. MainWindow Hotkey Handler (src/RaceOverlay.App/MainWindow.xaml.cs)
**Purpose**: Listens for CTRL+F12 hotkey globally and triggers drag mode toggle.

**Key Features**:
- `PreviewKeyDown` event handler on MainWindow
- Detects CTRL+F12 combination
- Calls `WidgetDragService.Instance.ToggleDragging()`
- Updates visual indicator
- `UpdateDragModeIndicator()` - Refreshes UI status bar

**Hotkey Detection**:
```csharp
if (e.Key == Key.F12 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
{
    WidgetDragService.Instance.ToggleDragging();
    UpdateDragModeIndicator();
}
```

### 4. MainWindowViewModel (src/RaceOverlay.App/ViewModels/MainWindowViewModel.cs)
**Purpose**: Enhanced to integrate WidgetDragService with widget lifecycle.

**Changes**:
- `ShowWidgetOverlay()` method now registers new windows with WidgetDragService
- Windows are tracked by the service for state synchronization

```csharp
WidgetDragService.Instance.RegisterWindow(overlayWindow);
```

### 5. Theme Updates (src/RaceOverlay.App/Themes/Theme.xaml)
**Purpose**: Added color definitions for drag mode indicator.

**New Colors**:
- `RO.Green` - #10B981 (Emerald green for "ON" state)
- `RO.Red` - #EF4444 (Red for "OFF" state)

**New Brushes**:
- `RO.GreenBrush` - For enabled state indicator
- `RO.RedBrush` - For disabled state indicator

### 6. MainWindow UI (src/RaceOverlay.App/MainWindow.xaml)
**Purpose**: Added status bar showing drag mode indicator.

**New Status Bar**:
- Located at top of MainWindow
- Shows drag mode status (ON/OFF) with colored indicator
- Displays helpful hint: "Press CTRL+F12 to toggle drag mode"
- Uses grid rows for proper layout positioning

**Status Indicator**:
- Green circle/text when drag mode is ON
- Red circle/text when drag mode is OFF

## User Experience

### Before (without hotkey)
1. Widgets fixed in their initial positions
2. Can only close and re-open to reposition
3. No interactive movement

### After (with CTRL+F12)
1. User presses CTRL+F12
2. All open widgets become draggable
3. Cursor changes to hand icon on widgets
4. User can click and drag widgets anywhere on screen
5. Status bar shows "Drag Mode: ON" with green indicator
6. Press CTRL+F12 again to disable drag mode
7. Widgets remain in their new positions
8. Status bar shows "Drag Mode: OFF" with red indicator

## Technical Details

### Architecture
- **Pattern**: Singleton service with event-based state management
- **Scope**: Global hotkey affects all open overlay windows simultaneously
- **State Persistence**: Windows maintain positions after drag mode is disabled
- **Cleanup**: Windows automatically unregister when closed

### Event Flow
1. User presses CTRL+F12 in MainWindow
2. MainWindow.PreviewKeyDown triggers handler
3. Handler calls WidgetDragService.Instance.ToggleDragging()
4. WidgetDragService toggles _isDraggingEnabled flag
5. WidgetDragService calls SetDraggingEnabled() on all registered windows
6. Each window updates its mouse event handlers and cursor
7. UI indicator updates to reflect new state

### Mouse Interaction
1. User clicks on widget content while drag mode enabled
2. MouseLeftButtonDown event captures mouse
3. Initial mouse position recorded
4. User moves mouse while button held
5. MouseMove event calculates delta from initial position
6. Window Left/Top properties updated with calculated offset
7. New mouse position becomes reference for next move
8. User releases mouse button
9. MouseLeftButtonUp releases mouse capture

## Build Status
✅ **All builds successful** - 0 errors, 0 warnings

## Files Modified
1. [src/RaceOverlay.App/Services/WidgetDragService.cs](src/RaceOverlay.App/Services/WidgetDragService.cs) - Created
2. [src/RaceOverlay.App/WidgetOverlayWindow.xaml.cs](src/RaceOverlay.App/WidgetOverlayWindow.xaml.cs) - Enhanced with drag support
3. [src/RaceOverlay.App/MainWindow.xaml.cs](src/RaceOverlay.App/MainWindow.xaml.cs) - Added hotkey handler
4. [src/RaceOverlay.App/MainWindow.xaml](src/RaceOverlay.App/MainWindow.xaml) - Added status bar
5. [src/RaceOverlay.App/ViewModels/MainWindowViewModel.cs](src/RaceOverlay.App/ViewModels/MainWindowViewModel.cs) - Window registration
6. [src/RaceOverlay.App/Themes/Theme.xaml](src/RaceOverlay.App/Themes/Theme.xaml) - Added green/red colors

## Testing

### Basic Functionality Test
1. Launch application
2. Select RelativeOverlay widget from list
3. Click "Add Widget" button
4. Overlay window appears on screen
5. Status bar shows "Drag Mode: OFF" (red indicator)
6. Press CTRL+F12
7. Status bar changes to "Drag Mode: ON" (green indicator)
8. Cursor changes to hand when over widget
9. Click and drag widget to new position
10. Press CTRL+F12 again
11. Status bar shows "Drag Mode: OFF" (red indicator)
12. Cursor returns to normal arrow
13. Widget stays in new position

### Multi-Widget Test
1. Add multiple instances of RelativeOverlay widget
2. Press CTRL+F12 to enable drag mode
3. All widgets become draggable simultaneously
4. Drag each widget to different positions
5. Press CTRL+F12 to disable drag mode
6. All widgets retain their new positions

## Future Enhancements
- Save widget positions to configuration file
- Add visual border highlight when drag mode is enabled
- Implement window snapping/alignment features
- Add drag mode timeout (auto-disable after period of inactivity)
- Remember window positions between app sessions
