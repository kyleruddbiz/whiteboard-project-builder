# Implementation Plan: Remove Title and Inspiration Button from Selector View

**Summary**: Remove the "Select Item Type" title text and the disabled Inspiration button from the WhiteboardItemSelectorView, simplifying the UI to show only Project Item and Someday Maybe buttons.

**Objectives**:
- Remove the "Select Item Type" header text from the selector view
- Remove the Inspiration button from the selector view
- Adjust the layout to accommodate the simplified UI (change from 2x2 grid to single row with two buttons)

**Constraints & Considerations**:
- The ViewModel does not have any Inspiration-related commands, so no ViewModel changes are required
- The layout should remain visually balanced after removing elements
- Follow existing XAML conventions (spaces in Margin/Padding values)

## Steps

### Step 1: Remove the Title TextBlock and Update Layout
- **What**: Remove the "Select Item Type" TextBlock header and simplify the Grid row definitions since the header row is no longer needed
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\Views\WhiteboardItemSelectorView.xaml`
- **Details**:
  - Remove the TextBlock element on lines 25-32 that displays "Select Item Type"
  - Remove the first RowDefinition (Height="Auto") from the outer Grid since no header exists
  - Update the inner button Grid to use Grid.Row="0" instead of Grid.Row="1" (or remove the Grid.Row attribute entirely)
  - Adjust the outer Grid margin as needed for visual balance
- **Validation**: Build the project and visually verify the title is no longer displayed

### Step 2: Remove the Inspiration Button and Simplify Button Grid
- **What**: Remove the disabled Inspiration button and restructure the button grid from 2x2 to a simpler layout with two buttons
- **Files**:
  - `C:\Users\TheTr\Code\Personal\whiteboard-project-builder\Views\WhiteboardItemSelectorView.xaml`
- **Details**:
  - Remove the entire Inspiration button element (lines 87-106 in current file)
  - Remove the comment about "Empty cell for future expansion" (line 108)
  - Change the button Grid from 2x2 to a single row with two columns:
    - Remove the second RowDefinition from the button Grid
    - Keep both ColumnDefinitions
  - Update button Grid.Row attributes:
    - Project Item button: Keep at Grid.Row="0", Grid.Column="0"
    - Someday Maybe button: Keep at Grid.Row="0", Grid.Column="1"
  - Consider adjusting vertical alignment of the button grid to center the buttons in the available space
- **Validation**: Build the project and visually verify only two buttons appear side by side

### Step 3: Verify Build and Test
- **What**: Ensure the application builds successfully and the selector view functions correctly
- **Files**:
  - None (verification only)
- **Details**:
  - Run `dotnet build` to verify no compilation errors
  - Launch the application and test that:
    - The selector view appears without the title
    - Only Project Item and Someday Maybe buttons are visible
    - Both buttons are clickable and function correctly
    - The Cancel (X) button still works
- **Validation**: Application builds and runs without errors; selector view displays correctly with only two buttons

## Testing & Verification
- Run `dotnet build` to confirm successful compilation
- Launch the application and trigger the selector view
- Verify the "Select Item Type" title is no longer visible
- Verify only the Project Item and Someday Maybe buttons are displayed
- Verify both buttons function correctly when clicked
- Verify the Cancel button (X) in top-right corner still works

## Notes
- The WhiteboardItemSelectorViewModel (`C:\Users\TheTr\Code\Personal\whiteboard-project-builder\ViewModels\WhiteboardItemSelectorViewModel.cs`) does not need any changes since it never had an Inspiration-related command
- The Inspiration button was already disabled (IsEnabled="False") and had no associated command
- This is a view-only change with no impact on application logic
