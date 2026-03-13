@echo off

set "buildDirectory=%~1"
set "toolVersion=%~2"
set "toolPackagePath=%~3"
set "outputDirectory=%~4"

set "toolName=plugintool"
set "toolCommand=plugin"

for /f "tokens=1,2" %%a in ('dotnet tool list') do (
    if "%%a"=="%toolName%" (
        if "%%b"=="%toolVersion%" (
            echo %toolName% version %toolVersion% is already installed.
            goto :run
        ) else (
            echo Found %toolName% with a different version: %%b
            echo Uninstalling %%b...
            dotnet tool uninstall %toolName%
        )
    )
)

echo Installing %toolName% version %toolVersion%...
dotnet tool install --add-source %toolPackagePath% %toolName% --version %toolVersion%

:run
echo Running %toolName% %buildDirectory% %outputDirectory%...

dotnet %toolCommand% pack -s %buildDirectory% -t %outputDirectory% -b 2>nul

if %ERRORLEVEL% equ 0 (
    echo mytool executed successfully!
) else (
    echo mytool encountered an error.
    exit /b 1
)

exit /b 0
