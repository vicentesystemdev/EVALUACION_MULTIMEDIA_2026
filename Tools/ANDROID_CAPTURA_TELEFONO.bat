@echo off
setlocal EnableExtensions
cd /d "%~dp0.."
rem Captura REAL de la pantalla del telefono via ADB.
rem Uso: Tools\ANDROID_CAPTURA_TELEFONO.bat 07_Controles_Tactiles
set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if not exist "%ADB%" set "ADB=adb"
set "NAME=%~1"
if "%NAME%"=="" set "NAME=captura_android"
set "OUT=Docs\Evaluacion_Dia2\Android\Evidencias\%NAME%.png"
if not exist "Docs\Evaluacion_Dia2\Android\Evidencias" mkdir "Docs\Evaluacion_Dia2\Android\Evidencias"
"%ADB%" exec-out screencap -p > "%OUT%"
for %%A in ("%OUT%") do if %%~zA LSS 1000 (
 echo ERROR: captura vacia. Comprueba "adb devices".
 del "%OUT%"
 exit /b 1
)
echo Captura guardada en %OUT%
