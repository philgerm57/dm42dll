@echo off
rem ==========================================================================
rem  build.bat : compile dm42dll (.NET Framework 4.7.2) et AppTestDll
rem
rem  Usage :  build.bat              compile la DLL (Debug) et AppTestDll
rem           build.bat release      idem en Release
rem           build.bat test         compile puis lance la validation (Tests\Validation)
rem           build.bat nopause      sans pause finale (pour un script)
rem           (options combinables : build.bat test nopause)
rem
rem  Resultat :
rem    bin\Debug\dm42dll.dll        utilisee par DM42editor et Py42
rem    Tests\Validation\validation-dm42dll.txt, VALID.free42, VALID.raw (option test)
rem ==========================================================================
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"
set "CONFIG=Debug"
set "TEST="
set "NOPAUSE="
for %%a in (%*) do (
    if /i "%%a"=="release" set "CONFIG=Release"
    if /i "%%a"=="test"    set "TEST=1"
    if /i "%%a"=="nopause" set "NOPAUSE=1"
)

echo.
echo ===== dm42dll : construction (%CONFIG%) =====

rem --- MSBuild de Visual Studio si present (projets .NET Framework), sinon dotnet
set "MSBUILD="
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "%VSWHERE%" (
    for /f "usebackq delims=" %%m in (`"%VSWHERE%" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do if not defined MSBUILD set "MSBUILD=%%m"
)
if defined MSBUILD (
    echo MSBuild : !MSBUILD!
    "!MSBUILD!" "dm42dll.sln" -nologo -v:minimal -restore -p:Configuration=%CONFIG% || goto :error
) else (
    where dotnet >nul 2>nul || (echo ERREUR : ni Visual Studio ni le SDK .NET trouves. & goto :error)
    echo MSBuild de Visual Studio introuvable : compilation avec dotnet.
    echo Le pack de developpement .NET Framework 4.7.2 doit etre installe.
    dotnet build "dm42dll.sln" -nologo -v:minimal -c %CONFIG% || goto :error
)

if not defined TEST goto :ok

rem --- validation encodage / decodage (programme .NET 10, lit bin\Debug\dm42dll.dll)
echo.
echo ===== Validation encodage / decodage =====
where dotnet >nul 2>nul || (echo ERREUR : le SDK .NET 10 est necessaire pour la validation. & goto :error)
rem la validation lit la DLL Debug : la compiler aussi apres un build release
if /i not "%CONFIG%"=="Debug" (
    if defined MSBUILD ("!MSBUILD!" "dm42dll.csproj" -nologo -v:minimal -p:Configuration=Debug || goto :error) else (dotnet build "dm42dll.csproj" -nologo -v:minimal -c Debug || goto :error)
)
dotnet run --project "Tests\Validation\Validation.csproj" -v:quiet -- "Tests\Validation"
set "RC=%ERRORLEVEL%"
echo Rapport : %~dp0Tests\Validation\validation-dm42dll.txt
if not "%RC%"=="0" (echo ERREUR : la validation a trouve des ecarts ^(voir le rapport^). & goto :error)

:ok
echo.
echo ===== Termine =====
echo DLL : %~dp0bin\%CONFIG%\dm42dll.dll
goto :done

:error
echo.
echo ===== ECHEC de la construction =====
if not defined NOPAUSE pause
exit /b 1

:done
if not defined NOPAUSE pause
exit /b 0
