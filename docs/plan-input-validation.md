# Plan: Add Input Validation on Settings

## Current State
- Widget config TextBoxes in `MainWindow.xaml` use `UpdateSourceTrigger=PropertyChanged` with no validation
- Users can enter negative numbers, non-numeric values, malformed hex colors, and out-of-range values
- Config classes are plain POCOs with no constraints

## Goal
Prevent invalid configuration values from being accepted, with clear visual feedback.

## Approach: WPF Validation Rules + ViewModel Guards

### Steps

1. **Add numeric validation to TextBox bindings**
   - Create `NumericValidationRule` (positive int) and `DoubleValidationRule` (positive double) in `App/Converters/` or `App/Validation/`
   - Apply via `<Binding.ValidationRules>` on relevant TextBoxes
   - WPF automatically shows red border on validation failure

2. **Add hex color validation**
   - Create `HexColorValidationRule` for color TextBoxes (must match `#RRGGBB` or `RRGGBB`)
   - Show red border + tooltip on invalid input

3. **Add range constraints to config properties**
   - Clamp values in property setters or use `CoerceValueCallback`
   - Key constraints:
     - Drivers Ahead/Behind: 1–30
     - Update intervals: 50–5000 ms
     - Tank capacity: 0.1–999 L
     - Opacity: 0–100%
     - Font sizes: 6–72

4. **Add validation error style**
   - Define a global `Validation.ErrorTemplate` in `App.xaml` that matches the dark theme
   - Show tooltip with error message on hover

5. **Prevent saving invalid configs**
   - Check `Validation.GetHasError()` before persisting
   - Or use `INotifyDataErrorInfo` on ViewModels for richer validation

### Files to Create/Modify
| File | Action |
|------|--------|
| `src/RaceOverlay.App/Validation/NumericValidationRule.cs` | Create |
| `src/RaceOverlay.App/Validation/DoubleValidationRule.cs` | Create |
| `src/RaceOverlay.App/Validation/HexColorValidationRule.cs` | Create |
| `src/RaceOverlay.App/App.xaml` | Modify — add error template style |
| `src/RaceOverlay.App/MainWindow.xaml` | Modify — add validation rules to TextBox bindings |

### Acceptance Criteria
- [ ] Numeric fields reject non-numeric and negative input with red border
- [ ] Color fields reject malformed hex values
- [ ] Values are clamped to sensible ranges
- [ ] Validation errors display tooltip with explanation
- [ ] Invalid configs are not persisted
