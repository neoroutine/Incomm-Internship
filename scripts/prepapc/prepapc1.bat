::Installation PC RM

::Changement de pagecode car de base, cmd n'aime pas les accents et les caractères spéciaux
chcp 65001
:welcome

::Saisi par l'utilisateur de l'emplacement USB car ce dernier peut changer
set /p WORKING=Ou se trouve le lecteur USB ? :
cd /d %WORKING%:\

::Stocke le nom de l'utilisateur admin actuel afin de pouvoir l'utiliser dans userFinish.bat
del admin.txt
set ADMIN=%USERNAME%
echo %ADMIN% > admin.txt

cls
@echo ************WELCOME %ADMIN%!************
@echo Installation complète (c) ou installation de composants précis (p)
@echo off

set /p INSTALL_TYPE=Veuillez saisir la lettre correspondant au choix : 
set TYPE=%INSTALL_TYPE:~,1%

if "%TYPE%" == "c" (goto start)
if "%TYPE%" == "p" (goto menu)

@echo Votre choix n'est pas valide, retour à la selection.
pause
goto welcome

:menu
@echo Veuillez choisir votre niveau d'installation :
@echo Installation complete (i)
@echo Copie INSTALL-INCOM (c)
@echo Installation de la suite de logiciel (s)
@echo Destruction et Creation d'utilisateur (u)
set /p CHOICE= Veuillez saisir la lettre correspondant au choix :
set CHAR=%CHOICE:~,1%
@echo off
if "%CHAR%" == "i" (goto start) 
if "%CHAR%" == "c" (goto copie)
if "%CHAR%" == "s" (goto suiteInstallation)
if "%CHAR%" == "u" (goto user)

@echo Vous n'avez choisi aucun niveau d'installation : retour au menu
pause

goto menu 

:start
@echo Service Informatique
@echo Travail dans %cd%
@echo.
@echo.
@echo off
::Propose à l'utilisateur de supprimer les repertoires à supprimer
@echo Veuillez supprimer ou garder les répertoires suivants :
@echo off
rmdir /s C:\ABINSTALL
rmdir /s C:\SAV

@echo.
@echo.

:copie
:: Copie Dossier Incomm dans C:/
@echo Copie des fichiers du dossier INSTALL-INCOMM
@echo off
rmdir /s C:\INSTALL-INCOMM
xcopy INSTALL-INCOMM C:\INSTALL-INCOMM
pause

@echo.
@echo.

@echo Voulez vous installer la suite de logiciel classique (O/N) ?
set /p ANSWER=
if "%ANSWER:~0%" == "o" (goto suiteInstallation) else (goto userCreation)

:suiteInstallation
@echo Lancement des installations de logiciels :

::Installation de Chrome
@echo Installation de Google Chrome. Merci de patienter...
@echo off

Start /w "" "/Chrome/Chrome/ChromeSetup.exe"
@echo Chrome est installé
pause

@echo.
@echo.

::Installation d'Adobe Reader
@echo Installation d'Adobe Reader. Merci de patienter...
@echo off
Start /w "" "Adobe/adobe.exe"
@echo Adobe Reader est installé
pause

@echo.
@echo.

::Installation du pack Office 2010
@echo Installation du pack Office 2010 Merci de patienter...
@echo off
Start /w "" "Office/Microsoft_Office_2010.exe"
Start /w "" "Office/Patch.exe"
@echo Office 2010 est installé
pause

@echo.
@echo. 

::Installation d'Anydesk
@echo Installation d'Anydesk
@echo off
Start /w "" "AnyDesk.exe"

@echo.
@echo.

::Installation manuelle d'OpenVPN
@echo Installation manuelle d'OpenVPN
openvpn-connect-3.2.3.1851_signed.msi
@echo off
pause

@echo.
@echo.

::Installation de Teamviewer
@echo Installation de TeamViewer. Merci de patienter...
@echo off
Start /w "" "Teamviewer/Teamviewer.exe" 
@echo TeamViewer est installé
pause

@echo.
@echo.

set /p CREATION=Voulez-vous creer/detruire un utilisateur (o/n)?
if "%CREATION:~,1%" == "n" (goto end)
:user
:: Destruction de l'ancien utilisateur et création du nouveau
@echo Destruction du compte user deja existant
@echo Nom du user deja existant : 
@echo off
::Saisi par l'utilisateur le nom de l'utilisateur à détruire
set /p OLDUSER=
powershell -command "Remove-LocalGroupMember -Group 'Utilisateurs' -Member '%OLDUSER%';"
net user %OLDUSER% /DELETE
rmdir /s C:\Users\%OLDUSER%
@echo %OLDUSER% detruit, bye bye !
@echo OFF

:: Création Compte Utilisateur USER
:: Chemins de base pour les profils et Répertoire Utilisateurs
:: Modifiez ces Variables si vous souhaitez les stocker ailleurs
SET PROFILS=C:\Users
SET BASE=C:\Users

:: Prise de Renseignements Utilisateur à créer

:: Autorisation de modification du mdp
SET CHPWD=NO
:: Activation du compte
SET ACT=YES

@echo.
@echo.

:: Définition des Variables
@echo off
set /p USER=Nom du user :
SET MDP=
del user.txt
echo %USER% > user.txt 
SET PROF=%PROFILS%\%USER%
SET HD=%BASE%\%USER%

::Création du nouvel utilisateur
net user %USER% /add
@echo Creation de %USER% TERMINEE 
pause

:wupdate
Start /w "" "Windows10Upgrade9252.exe"

:end
set /p DISC=La première partie du script est terminee, voulez-vous vous deconnecter afin d'executer la deuxieme partie sur le compte rm/user (o/n) ? : 
if "%DISC:~,1%" == "o" (shutdown -L) else (exit)

:: Source : http://support-fr.org/dim/2015/05/18/batch-user-add/
:: Aide : https://initscreen.developpez.com/tutoriels/batch/apprendre-la-programmation-de-script-batch/#LII-B
:: groupe utilisateur Windows https://www.windows-commandline.com/list-of-user-groups-command-line/
:: Et beaucoup de connaissances très pointues de la part d'un stagiaire