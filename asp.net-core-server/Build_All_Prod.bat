@echo off
REM Help: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#linux-rids
REM Don't forget to manually copy "chmod.txt" into the Linux & Mac build .zips!
REM Also, remember that the only folder needed from the builds is the "publish" folder. I rename this folder to "Heatdeath Backend-0.4.2 (Win64)" or whatever OS it is for.
REM     In the example above the path to the "publish" folder would be: Backend\bin\Release\netcoreapp2.2\win-x64\publish

echo Starting builds...
call dotnet publish -r win-x86 -c Release --self-contained true
call dotnet publish -r win-x64 -c Release --self-contained true
call dotnet publish -r linux-x64 -c Release --self-contained true
call dotnet publish -r linux-arm -c Release --self-contained true
call dotnet publish -r osx-x64 -c Release --self-contained true