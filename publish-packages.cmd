@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command ".\publish-packages.ps1 -nuget -github %*"
exit /b %ErrorLevel%
