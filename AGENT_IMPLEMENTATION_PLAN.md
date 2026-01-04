# Implementation Plan: Modify WhiteboardItemSelectorView for 4 Evenly-Spaced Buttons

**Summary**: Update the WhiteboardItemSelectorView to display 4 buttons (2 rows x 2 columns) that evenly fill the available vertical space within the 800px tall container.

**Objectives**:
- Add a 2x2 grid layout to accommodate 4 buttons
- Make all 4 buttons evenly distribute across the available vertical space
- Maintain the current width behavior (buttons stretch horizontally within their columns)
- Add placeholder buttons for Goal and Inspiration item types (existing enum values)

**Constraints & Considerations**:
- The container is a fixed 502x800 Border with 40px margin on the inner Grid
- Current buttons are in a single-row, 2-column layout
- The inner Grid has VerticalAlignment="Center" which must be changed to "Stretch" for even distribution
- Must use RowDefinitions with `Height="*"` for equal vertical distribution
- The enum `WhiteboardItemType` already has Goal and Inspiration values
- Must follow MVVM pattern and project XAML guidelines

## Steps

### Step 1: Update WhiteboardItemSelectorView.xaml Grid Layout
- **What**: Modify the inner Grid to use a 2x2 layout with equal row heights
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\Views\WhiteboardItemSelectorView.xaml`
- **Details**:
  - Change the inner Grid's `VerticalAlignment` from `Center` to `Stretch` (or remove it entirely since Stretch is default)
  - Add two RowDefinitions with `Height="*"` each for equal vertical distribution
  - Keep the existing two ColumnDefinitions with `Width="*"`
  - Move the "Project Item" button to `Grid.Row="0" Grid.Column="0"`
  - Move the "Someday Maybe" button to `Grid.Row="0" Grid.Column="1"`
  - Add a "Goal" button at `Grid.Row="1" Grid.Column="0"`
  - Add an "Inspiration" button at `Grid.Row="1" Grid.Column="1"`
- **Validation**: Run `dotnet build` to verify XAML compiles correctly

### Step 2: Add Goal Button Content
- **What**: Create the Goal button with appropriate icon and label
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\Views\WhiteboardItemSelectorView.xaml`
- **Details**:
  - Follow the existing button pattern with StackPanel containing FontIcon and TextBlock
  - Use an appropriate goal-related icon (e.g., `&#xE734;` - Target/Bullseye, or `&#xE8FB;` - Flag)
  - Set Text="Goal" on the TextBlock
  - Match existing button styling: `Margin="10"`, `Padding="20"`, `HorizontalAlignment="Stretch"`, `VerticalAlignment="Stretch"`
  - Bind Command to `ViewModel.SelectGoalItemCommand` (to be added in next step)
- **Validation**: XAML structure matches existing buttons

### Step 3: Add Inspiration Button Content
- **What**: Create the Inspiration button with appropriate icon and label
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\Views\WhiteboardItemSelectorView.xaml`
- **Details**:
  - Follow the existing button pattern with StackPanel containing FontIcon and TextBlock
  - Use an appropriate inspiration-related icon (e.g., `&#xE7B5;` - Lightbulb, or `&#xE945;` - Idea)
  - Set Text="Inspiration" on the TextBlock
  - Match existing button styling: `Margin="10"`, `Padding="20"`, `HorizontalAlignment="Stretch"`, `VerticalAlignment="Stretch"`
  - Bind Command to `ViewModel.SelectInspirationItemCommand` (to be added in next step)
- **Validation**: XAML structure matches existing buttons

### Step 4: Add SelectGoalItem Command to ViewModel
- **What**: Add the RelayCommand for selecting Goal item type
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\ViewModels\WhiteboardItemSelectorViewModel.cs`
- **Details**:
  - Add a new `[RelayCommand]` attributed method named `SelectGoalItem`
  - Method should invoke `ItemTypeSelected?.Invoke(this, WhiteboardItemType.Goal);`
  - Follow the existing pattern used by `SelectProjectItem` and `SelectSomedayMaybeItem` methods
- **Validation**: Run `dotnet build` to verify the command is generated correctly

### Step 5: Add SelectInspirationItem Command to ViewModel
- **What**: Add the RelayCommand for selecting Inspiration item type
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\ViewModels\WhiteboardItemSelectorViewModel.cs`
- **Details**:
  - Add a new `[RelayCommand]` attributed method named `SelectInspirationItem`
  - Method should invoke `ItemTypeSelected?.Invoke(this, WhiteboardItemType.Inspiration);`
  - Follow the existing pattern used by `SelectProjectItem` and `SelectSomedayMaybeItem` methods
- **Validation**: Run `dotnet build` to verify the command is generated correctly

## Testing & Verification
1. Run `dotnet build` to ensure no compilation errors
2. Run the application with `dotnet run`
3. Click the "+" add item button to open the WhiteboardItemSelectorView
4. Verify that:
   - All 4 buttons are visible in a 2x2 grid layout
   - Buttons evenly distribute vertically (each row takes ~50% of available height minus margins)
   - Buttons stretch horizontally within their respective columns
   - Each button has an appropriate icon and label
   - Clicking the X button still closes the selector
5. Note: The Goal and Inspiration buttons will raise events but may not have handlers in MainPage yet - this is expected for this implementation

## Notes
- The `WhiteboardItemType` enum already contains `Goal` and `Inspiration` values, so no changes needed there
- The event handlers in the consuming code (MainPage) may need to be updated separately to handle the new item types, but that is outside the scope of this task
- The fixed dimensions of the container (502x800) with 40px margin leaves approximately 720px of vertical space for the buttons to share
- With the 2x2 grid using `Height="*"` rows, each row will get approximately 360px of height, and with 10px margin on each button, the actual button height will be approximately 340px per button

## Current Code Reference

### Current XAML Structure (WhiteboardItemSelectorView.xaml):
```xml
<Grid Margin="40" VerticalAlignment="Center">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <!--  Project Item Button  -->
    <Button Grid.Column="0" ... />

    <!--  Someday Maybe Button  -->
    <Button Grid.Column="1" ... />
</Grid>
```

### Target XAML Structure:
```xml
<Grid Margin="40">
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <!--  Project Item Button  -->
    <Button Grid.Row="0" Grid.Column="0" ... />

    <!--  Someday Maybe Button  -->
    <Button Grid.Row="0" Grid.Column="1" ... />

    <!--  Goal Button  -->
    <Button Grid.Row="1" Grid.Column="0" ... />

    <!--  Inspiration Button  -->
    <Button Grid.Row="1" Grid.Column="1" ... />
</Grid>
```

### Current ViewModel Commands (WhiteboardItemSelectorViewModel.cs):
```csharp
[RelayCommand]
private void SelectProjectItem()
{
    ItemTypeSelected?.Invoke(this, WhiteboardItemType.Project);
}

[RelayCommand]
private void SelectSomedayMaybeItem()
{
    ItemTypeSelected?.Invoke(this, WhiteboardItemType.SomedayMaybe);
}
```
