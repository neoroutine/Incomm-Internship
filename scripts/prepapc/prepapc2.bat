@echo off
::Stop Edge car il peut empêcher le bon fonctionnement du script
powershell -command "Stop-Process -Name 'msedge'"
::Changement de codepage car cmd ne gère pas très bien les accents et les caractères spéciaux
chcp 65001
::Saisi par l'utilisateur de l'emplacement USB car ce dernier peut changer
set /p WORKING=Ou se trouve le lecteur USB ? :
cd /d %WORKING%:\

::Confirmation que l'utilisateur est bien sur la session user
set /p ONUSERACC=Etes vous bien sur la session user/rm (o/n)? 
if "%ONUSERACC:~, 1%" == "o" (@echo Parfait !) else (exit)
pause

cls

::Récuperation des informations stockées au préalable (nom de l'admin et nom du user)
for /f %%n in (admin.txt) do set ADMIN=%%n
for /f %%n in (user.txt) do set USER=%%n
@echo *****WELCOME BACK*****


:: Copie des favoris contenant = Lelog & Webmail
@echo Copie des favoris Chrome LeLog et Webmail chez %USER%
@echo off
xcopy "Chrome\Bookmarks" "C:\Users\%USER%\AppData\Local\Google\Chrome\User Data\Default\"
pause

@echo(
@echo(

::Installation de Teams
@echo Installation de Teams. Merci de patienter...
@echo off

Start /w "" "/INSTALL-INCOMM/Teams_windows_x64"

:: Copie des Raccourcis du Pack Office 2010
@echo Copie des Raccourcis Pack Office 2010 sur le bureau de %USER%
@echo off
powershell -command "Copy-Item -Path 'C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Microsoft Office Starter (Français)\*' -Destination 'C:\Users\%USER%\Desktop' -PassThru -Recurse"
xcopy "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Word.lnk" "C:\Users\%USER%\Desktop\"
xcopy "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Excel.lnk" "C:\Users\%USER%\Desktop\"
pause

@echo(
@echo(

@echo Copie du raccourci Open VPN Connect sur le bureau de %USER%
@echo off
xcopy "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\OpenVPN Connect\OpenVPN Connect.lnk" "C:\Users\%USER%\Desktop\"
pause

@echo(
@echo(


:: Copie les fond d'écrans et le theme utilisés par le PC
@echo Copie des fond d'écrans ainsi que le thème utilisés par le PC
@echo off
xcopy INSTALL-INCOMM\fond-ecran C:\Users\%USER%\Documents
xcopy "INSTALL-INCOMM\Theme Incomm.theme" C:\Users\%USER%\Documents
pause

@echo(
@echo(

:: Execute le theme afin qu'il se mette en place sur le PC
@echo Installation du theme Incomm
Start /w "" "C:\Users\%USER%\Documents\Theme Incomm.theme"

@echo(
@echo(


:: Telecharge le .png du QRcode associé au numéro de série du PC puis le copie dans la racine et sur le bureau
:: iwr = Invoke Web Request, outf = output file
@echo QRcode generator and perms
@echo off
SET /p SN=Veuillez entrer le numéro série du PC (%COMPUTERNAME%) :
powershell -command "iwr -outf 'C:\INSTALL-INCOMM\%SN%.png' 'https://api.qrserver.com/v1/create-qr-code/?size=500x500&data=%SN%'"
pause

@echo off

::Créer un raccourci sur le bureau avec comme source le QRCode généré à la racine de INSTALL-INCOMM
powershell -command "$WshShell = New-Object -comObject WScript.Shell;"^
                    "$Shortcut = $WshShell.CreateShortcut('C:\Users\%USER%\Desktop\%SN%.lnk');"^
					"$Shortcut.TargetPath = 'C:\INSTALL-INCOMM\%SN%.png';"^
					"$Shortcut.Save();"
				
@echo Raccourci cree !
@echo off

::Gère les permissions du raccourci afin que l'utilisateur ne puisse pas le détruire par mégarde, demander au stagiaire pour plus d'infos :)
powershell -command "$acl = get-acl -Path 'C:\Users\%USER%\Desktop\%SN%.lnk'; $owner = New-Object System.Security.Principal.NTAccount '%COMPUTERNAME%\admin';"^
                    "$acl.setAccessRuleProtection($true,$false);"^
                    "$acl.setOwner($owner);"^
                    "$arFC = New-Object System.Security.AccessControl.FilesystemAccessRule('%COMPUTERNAME%\%ADMIN%', 'FullControl', 'Allow');"^
                    "$acl.setAccessRule($arFC);"^
                    "$arRE = New-Object System.Security.AccessControl.FilesystemAccessRule('%COMPUTERNAME%\%USER%', 'ReadAndExecute', 'Allow');"^
                    "$arWrite = New-Object System.Security.AccessControl.FilesystemAccessRule('%COMPUTERNAME%\%USER%', 'Write', 'Deny');"^
                    "$arDel = New-Object System.Security.AccessControl.FilesystemAccessRule('%COMPUTERNAME%\%USER%', 'Delete', 'Deny');"^
                    "$acl.setAccessRule($arRE);"^
                    "$acl.setAccessRule($arWrite);"^
                    "$acl.setAccessRule($arDel);"^
                    "$acl | Set-Acl -Path 'C:\Users\%USER%\Desktop\%SN%.lnk'"
					
@echo Permissions gerees
@echo off

::Droit de lecture
attrib +R "C:\Users\%USER%\Desktop\%SN%.lnk"
pause

@echo(
@echo(

@echo Le PC est configuré, merci de vous assurer que tout soit conforme !
@echo off
pause
