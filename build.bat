@ECHO OFF
PUSHD %~dp0

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\build.ps1' %*";

IF %errorlevel% neq 0 PAUSE
