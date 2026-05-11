# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WhiteboardProjectBuilder is a WinUI 3 desktop application built on .NET 9.0, targeting Windows 10.0.19041.0 (Windows 10 version 2004) and higher.

### Application Purpose

This application creates printable whiteboard items for use on physical whiteboards. Key features include:

- **Item Types**: Currently Project items and Task items. Each item has both a Type (`WhiteboardItemType`) and a Size (`WhiteboardItemSize`); these are orthogonal axes that the template selector keys on as a tuple. Which types support which sizes is declared in `Enums/WhiteboardItemSize.cs` (`SupportedItemTypes` extension method).
- **Sizes**: `Small` (502×400), `Medium` (502×800), `Large` (1004×800), `Huge` (1004×1600). Sizes are multiples of 2 along one axis so they tile cleanly into the print page (a 2×2 grid of Medium cells).
- **Largest-first ordering**: Home-page sections and print slots are ordered by descending size (Huge → Large → Medium → Small). The `WhiteboardItemSize` enum is declared smallest-to-largest, so this is just `OrderByDescending(size => size)`. This is a packing rule (rocks-then-sand), not aesthetics — preserve it when adding new sizes or layout/print code.
- **Editable Items**: Each whiteboard item can be edited based on its template.
- **List Management**: Users add items via per-size Add buttons and can remove them from the list.
- **Print Functionality**: Print whiteboard items with up to four Medium-sized cells per page (Smalls pair into a Medium-sized slot; Large/Huge support is not yet wired into the print packer).

## Development Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Clean
```bash
dotnet clean
```

### Restore packages
```bash
dotnet restore
```

### Git Commands
Use the GitHub CLI (`gh`) and standard git commands for version control:

```bash
# Fetch latest changes
git fetch

# Commit changes
git add .
git commit -m "commit message"

# Push changes
git push

# Review history
git log
git log --oneline
git log --graph --oneline --all
```

## Architecture

### Technology Stack
- **Framework**: .NET 9.0
- **UI Framework**: WinUI 3 (via Microsoft.WindowsAppSDK)
- **Target Platform**: Windows 10 (19041) minimum, with support for x86, x64, and ARM64
- **Language**: C# with nullable reference types enabled, implicit usings, and latest language version

### Project Structure
- `App.xaml` / `App.xaml.cs` - Application entry point and lifecycle management
- `Views/` - XAML pages/controls and their code-behind files
  - `MainPage.xaml` / `MainPage.xaml.cs` - Main application page
  - `ProjectItemView.xaml` / `ProjectItemView.xaml.cs` - Project item UserControl (current Medium size)
  - `TaskItemView.xaml` / `TaskItemView.xaml.cs` - Task item UserControl (current Small size)
  - Card UserControls do **not** pin their own `Width`/`Height` — the host (DataTemplate cell or print slot) supplies the layout footprint per `WhiteboardItemSize`.
- `ViewModels/` - MVVM view models (using CommunityToolkit.Mvvm)
- `Models/` - Data models for whiteboard items
- `Enums/` - Enumerations with their extension methods
- `Converters/` - XAML value converters for data binding
  - `StringToImageSourceConverter.cs` - Converts string paths to ImageSource with ms-appx:/// URI
- `Assets/` - Application images and resources (logos, splash screen)
- `Examples/` - PDF files with examples of expected output and design references
- `Properties/` - Assembly information and configuration
- `Package.appxmanifest` - Windows app package manifest
- `app.manifest` - Application manifest for Windows compatibility
- `Imports.cs` - Global using directives

### Key Dependencies
- **Microsoft.WindowsAppSDK** (v1.*) - Core WinUI 3 framework
- **Microsoft.Web.WebView2** (v1.*) - Embedded web view control
- **Microsoft.Windows.SDK.BuildTools** (v10.*) - Windows SDK build tools
- **CommunityToolkit.Mvvm** (v8.*) - MVVM toolkit for observable objects and commands

### Platform Support
Supports building for multiple architectures: x86, x64, and ARM64 on Windows 10 and later.

## Coding Standards

### Code Style
- Write clear and concise code
- Use using directives instead of fully qualified names (unless there is a naming conflict)
- Follow the MVVM (Model-View-ViewModel) pattern
- Use nullable reference types throughout the codebase
- Extension methods for enums should be defined in the same file as the enum
- Include "Async" suffix on async methods
- Use var with linq, annoymous types, and when the type is apparent
- Avoid var for built-in types
- **Prefer primary constructors** for services, repositories, and ViewModels whose constructor only captures parameters as fields. Use a traditional constructor body only when initialization logic is needed beyond capture (e.g., wiring events, computing derived state, calling base methods with non-pass-through arguments).

### MVVM Guidelines
- **Never use `x:Name` to reference UserControls** - Avoid directly accessing UserControl instances in code-behind
- **Always use data binding** - Pass ViewModels to UserControls via DependencyProperty bindings, not constructor parameters or property setters
- **UserControl ViewModels must be DependencyProperties** - Use `DependencyProperty.Register` for ViewModel properties in UserControls, not get-only properties
- **DP-backed `ViewModel` properties are typed non-nullable** - For a UserControl's `ViewModel` (or other required-by-binding) DP, declare the CLR getter as non-nullable and suppress the framework-boundary null with `!`: `get => (TViewModel)GetValue(ViewModelProperty)!;`. Keep `new PropertyMetadata(null)` — the suppression is a public-surface assertion that the binding has been applied before any consumer reads. This eliminates `if (ViewModel != null)` clutter in code-behind handlers; input/event handlers only fire after the template engine has applied the binding, so the null window is unreachable in practice. If code does access the property before binding evaluates (e.g., from a constructor), it will NRE loud and fast — the correct failure mode. This matches the project's existing `null!` convention for DI- and binding-initialized fields.
- **Set DataContext in UserControl only when needed** - Set `this.DataContext = this` only if the UserControl uses plain `{Binding}` internally. If every internal binding is `{x:Bind}` (which targets the code-behind, not DataContext), `DataContext = this` is redundant and actively harmful: it overrides the DataContext a parent template would otherwise inherit, breaking parent-side `{Binding}` expressions that target this control's DependencyProperties (e.g. `ViewModel="{Binding}"` from a hosting `DataTemplate`).

### Comments
- Keep comments clear and concise
- Only add comments when the code's intent is not obvious
- Do not add comments to describe basic or obvious functionality
- Do add a comment to empty catch or if blocks explaining why

### XAML Guidelines
- Prefer `x:Bind` over `Binding` whenever possible for better performance and compile-time checking
- **Exception: `ContentControl` + `ContentTemplateSelector` boundary** - When a `ContentControl` hosts CLR ViewModels and dispatches templates by runtime type (`ContentTemplateSelector`), the inner `DataTemplate` root must use plain `{Binding}` (not `{x:Bind}`) to receive the typed VM. `ContentControl.Content` marshals managed objects through WinRT's `IInspectable` projection; `x:Bind`'s compile-time cast cannot unwrap it and throws `InvalidCastException`. Plain `{Binding}` resolves through the DP system at runtime, which handles the unwrap. Don't add `x:DataType` to these inner templates — it generates the same failing cast.
- Use spaces (not commas) to separate values in Margin and Padding attributes (e.g., `Margin="10 20 10 20"` not `Margin="10,20,10,20"`)
- **Image binding in WinUI 3** - Always use `StringToImageSourceConverter` when binding string paths to Image.Source properties. WinUI 3 requires proper `ms-appx:///` URI scheme for packaged assets. Example: `Source="{x:Bind ViewModel.ImagePath, Mode=OneWay, Converter={StaticResource StringToImageSourceConverter}}"`
- **Conditional visibility** - Always use `x:Load` instead of `Visibility` for conditional element rendering. Elements using `x:Load` MUST have an `x:Name` attribute. Example: `<Button x:Name="MyButton" x:Load="{x:Bind ViewModel.IsVisible, Mode=OneWay}" />`
- **Keyboard accelerators** - Always use `KeyboardAcceleratorCommandBehavior` instead of `Invoked` event handlers. This follows the MVVM pattern by binding commands from ViewModels. The behavior supports both `Command` and optional `CommandParameter` properties. Example: `<KeyboardAccelerator Key="V" Modifiers="Control" behaviors:KeyboardAcceleratorCommandBehavior.Command="{x:Bind ViewModel.PasteImageCommand}" behaviors:KeyboardAcceleratorCommandBehavior.CommandParameter="{x:Bind XamlRoot}" />`

### Development Workflow
- Always run `dotnet build` after implementation to verify changes
