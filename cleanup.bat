@echo off
echo Removing non-English satellite assemblies...
for /d %%i in (publish\*) do (
    if /i not "%%~ni" == "en" (
        if exist %%i\*.resources.dll rmdir /s /q "%%i"
    )
)
echo Done.