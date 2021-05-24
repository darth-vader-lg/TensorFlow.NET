@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command ".\publush-packages.ps1 -nuget -github %*"
exit /b %ErrorLevel%
