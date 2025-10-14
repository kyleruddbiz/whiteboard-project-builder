@echo off
REM Pre-commit hook that runs dotnet format on staged files (Windows batch version)

echo Running dotnet format on staged files...

REM Run dotnet format on the entire solution
dotnet format WhiteboardProjectBuilder.sln >nul 2>&1

if %errorlevel% neq 0 (
    echo dotnet format failed. Please fix the issues and try again.
    exit /b 1
)

echo dotnet format completed successfully.
exit /b 0
