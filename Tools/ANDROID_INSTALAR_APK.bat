@echo off
setlocal EnableExtensions
cd /d "%~dp0.."
rem Instala Builds\Android\CampusGlitch.apk en el telefono conectado por USB (Depuracion USB activada).
set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if not exist "%ADB%" set "ADB=adb"
if not exist "Builds\Android\CampusGlitch.apk" (
 echo ERROR: no existe Builds\Android\CampusGlitch.apk. Generalo en Unity: Campus Glitch ^> Android ^> 2. Generar APK
 pause
 exit /b 1
)
"%ADB%" devices
"%ADB%" install -r "Builds\Android\CampusGlitch.apk"
if errorlevel 1 (
 echo ERROR: no se pudo instalar. Revisa que el telefono acepte la depuracion USB.
 pause
 exit /b 2
)
"%ADB%" shell monkey -p com.unifranz.campusglitch -c android.intent.category.LAUNCHER 1
echo APK instalado y lanzado.
pause
