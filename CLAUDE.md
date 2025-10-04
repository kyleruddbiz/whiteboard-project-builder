# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WhiteboardProjectBuilder is a WinUI 3 desktop application built on .NET 9.0, targeting Windows 10.0.19041.0 (Windows 10 version 2004) and higher.

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
- `Assets/` - Application images and resources (logos, splash screen)
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
