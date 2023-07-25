@echo off

set "toolName=%~1"
set "toolVersion=%~2"
set "toolPackagePath=%~3"

set "isInstalled="
for /f "tokens=*" %%i in ('dotnet tool list') do (
    if "%%i" == "!toolName!" (
        set "isInstalled=true"
    )
)

if defined isInstalled (
    REM Tool is installed, uninstall it first
    dotnet tool uninstall %toolName%
)

dotnet tool install --add-source %toolPackagePath% %toolName% --version %toolVersion%
