@echo off
chcp 65001
powershell "echo Bonjour !;"
setlocal DISABLEDELAYEDEXPANSION
cd %~dp0
set CURRSITE=example.com

set user=domain\admin
set pwd=password


:nas
net use X: /delete
net use X: \\xxx.xxx.x.xx\Aspis /user:%user% %pwd% && goto welcome
goto nas

powershell "cls"

:welcome
@echo *****ASPIRATEUR DE SITES*****
@echo.
@echo.

if not exist "sites.txt" ( echo Afin de commencer l'aspiration, vous devez fournir un fichier nommé sites.txt contenant une liste d'url/noms de domaine, un site par ligne & goto end)

:LOR
set /p LOR=Voulez vous stocker les sites aspires sur le [NAS] ou en [Local] (N/L) ?
if "%LOR:~0,1%" == "N" (set "CURRDIR=X:" & goto pause)
if "%LOR:~0,1%" == "n" (set "CURRDIR=X:" & goto pause)
if "%LOR:~0,1%" == "L" (set "CURRDIR=%~dp0aspirations" & goto pause)
if "%LOR:~0,1%" == "l" (set "CURRDIR=%~dp0aspirations" & goto pause)

@echo.
@echo.

:pause
set /p CHOIX=Faire une pause entre chaque aspiration (O/N) ?
if "%CHOIX:~0,1%" == "O" (set PAUSE=1 & goto start)
if "%CHOIX:~0,1%" == "o" (set PAUSE=1 & goto start)
if "%CHOIX:~0,1%" == "N" (set PAUSE=0 & goto start)
if "%CHOIX:~0,1%" == "n" (set PAUSE=0 & goto start)

@echo Votre choix n'est pas valide, retour à la selection.
pause
goto pause

@echo.
@echo.

:start
setlocal EnableDelayedExpansion
echo Debut de l'aspiration
for /f "tokens=1 delims=" %%a in (sites.txt) do (
   set CURRSITE=%%a
   set NOTSITES=";"
   
   if  "x!CURRSITE:.=!"=="x!CURRSITE!" echo "!CURRSITE!" n'est pas un ndd valide, au suivant ! & set NOTSITES=!NOTSITES!;!CURRSITE!
   
   if not "x!CURRSITE:.=!"=="x!CURRSITE!" (
   if "!CURRSITE:~0,8!" == "https://" ( if "!CURRSITE:~-1!" == "/" (set CURRSITE=!CURRSITE:~8,-1!) )
   if "!CURRSITE:~0,7!" == "http://" ( if "!CURRSITE:~-1!" == "/" (set CURRSITE=!CURRSITE:~7,-1!) )
   
   if "!CURRSITE:~0,4!" == "www." (set CURRSITE=!CURRSITE:~4!)
   
   echo Aspiration de !CURRSITE!
   
   if not exist "%CURRDIR%\!CURRSITE!" mkdir "%CURRDIR%\!CURRSITE!"
   
   "httrack/httrack.exe" %%a -O "%CURRDIR%\!CURRSITE!" "+*!CURRSITE!\* -v
   )
   
   :eol
   if %PAUSE% == 1 (pause)
   @echo.
   @echo.
   
)

:end
net use X: /delete
echo !NOTSITES:;=&echo.!
echo Fini, veuillez sortir du script
pause