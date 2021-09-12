@echo off
chcp 65001
powershell "echo Bonjour !;cls;"


@echo (Re)Installation de CD-PROSPECT
@echo Veuillez appuyer pour continuer à chaque fin d'étape (4 étapes en tout)
@echo Merci et bonne installation !

set ADMIN="admin"
set PASWD="password"

set DESTROY='Install-Package nuget -Force; Uninstall-Package CD-PROSPECT -Force; Remove-Item -Path C:\CD-PROSPECT -Recurse -Force; mkdir C:\CD-PROSPECT;'
set DOWNLOAD='iwr -outf C:\CD-PROSPECT\a.zip https://www.xxxxx.xxx/cdprospect.zip;'
set INSTALL='Expand-Archive -Path C:\CD-PROSPECT\a.zip -DestinationPath C:\CD-PROSPECT\;start-process -NoNewWindow -FilePath ""C:\CD-PROSPECT\install.exe"";'
set CLEANUP='Remove-Item -Path C:\CD-PROSPECT\a -Force; Remove-Item -Path C:\CD-PROSPECT\a.zip -Force;'

.\RunAsSpc.exe /program:"C:\WINDOWS\system32\cmd.exe" /param:"/C powershell -command start-process powershell -ArgumentList %DESTROY% -verb runas" /user:%ADMIN% /password:%PASWD% > nul 2>&1
@echo.
@echo.
@echo ETAPE 1(DESTRUCTION) FINIE, VEUILLEZ APPUYER SUR UNE TOUCHE POUR CONTINUER L'INSTALLATION DE CD-PROSPECT
pause
.\RunAsSpc.exe /program:"C:\WINDOWS\system32\cmd.exe" /param:"/C powershell -command start-process powershell -ArgumentList %DOWNLOAD% -verb runas" /user:%ADMIN% /password:%PASWD% > nul 2>&1
@echo.
@echo.
@echo ETAPE 2(EXTRACTION) FINIE, VEUILLEZ APPUYER SUR UNE TOUCHE POUR CONTINUER L'INSTALLATION DE CD-PROSPECT
pause
.\RunAsSpc.exe /program:"C:\WINDOWS\system32\cmd.exe" /param:"/C powershell -command start-process powershell -ArgumentList %INSTALL% -verb runas" /user:%ADMIN% /password:%PASWD% > nul 2>&1
@echo.
@echo.
@echo ETAPE 3(INSTALLATION) FINIE, VEUILLEZ APPUYER SUR UNE TOUCHE POUR CONTINUER L'INSTALLATION DE CD-PROSPECT
pause
.\RunAsSpc.exe /program:"C:\WINDOWS\system32\cmd.exe" /param:"/C powershell -command start-process powershell -ArgumentList %CLEANUP% -verb runas" /user:%ADMIN% /password:%PASWD% > nul 2>&1
@echo.
@echo.
@echo ETAPE 4(NETTOYAGE) FINIE, INSTALLATION FINIE !
pause
