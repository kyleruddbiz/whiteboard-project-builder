# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WhiteboardProjectBuilder is a WinUI 3 desktop application built on .NET 9.0, targeting Windows 10.0.19041.0 (Windows 10 version 2004) and higher.

### Application Purpose

This application creates printable whiteboard items for use on physical whiteboards. Key features include:

- **Multiple Templates**: Projects, Goals, and Inspiration templates
- **Standardized Size**: All whiteboard items are the same size with template-specific content
- **Editable Items**: Each whiteboard item can be edited based on its template
- **List Management**: Users can add and remove whiteboard items from a list
- **Print Functionality**: Print whiteboard items with up to four items per page

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
- `Views/` - XAML pages and their code-behind files
  - `MainPage.xaml` / `MainPage.xaml.cs` - Main application page
- `Models/` - Data models for whiteboard items
- `Enums/` - Enumerations with their extension methods
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

### Platform Support
Supports building for multiple architectures: x86, x64, and ARM64 on Windows 10 and later.

## Coding Standards

### Code Style
- Write clear and concise code
- Follow the MVVM (Model-View-ViewModel) pattern
- Use nullable reference types throughout the codebase
- Extension methods for enums should be defined in the same file as the enum

### Comments
- Keep comments clear and concise
- Only add comments when the code's intent is not obvious
- Do not add comments to describe basic or obvious functionality

### XAML Guidelines
- Prefer `x:Bind` over `Binding` whenever possible for better performance and compile-time checking

### Development Workflow
- Always run `dotnet build` after implementation to verify changes
