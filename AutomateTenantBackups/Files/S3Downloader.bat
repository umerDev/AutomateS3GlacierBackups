@echo off

title AWS Backup
echo Welcome to AWS Backup



color a
echo db    db .88b  d88. d88888b d8888b.
echo	88    88 88'YbdP`88 88'     88  `8D
echo	88    88 88  88  88 88ooooo 88oobY'
echo	88    88 88  88  88 88~~~~~ 88`8b  
echo	88b  d88 88  88  88 88.     88 `88.
echo	~Y8888P' YP  YP  YP Y88888P 88   YD               
echo.

color a

::Backup dev buckets
echo You are going to copy bucket amplify-ar-development
aws s3 sync s3://amplify-ar-development %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today
echo Finished download
setlocal enableextensions
set todaysDate=%DATE:/=_%
Rename %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today amplify-ar-development_"%todaysDate%"
echo Renamed Folder

::Backup demo buckets
echo You are going to copy bucket ar-demo-tenants
aws s3 sync s3://ar-demo-tenants %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today
echo Finished download
setlocal enableextensions
set todaysDate=%DATE:/=_%
Rename %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today ar-demo-tenants_"%todaysDate%"
echo Renamed Folder

::Backup PoC buckets
echo You are going to copy bucket ar-poc-tenants
aws s3 sync s3://ar-poc-tenants %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today
echo Finished download
setlocal enableextensions
set todaysDate=%DATE:/=_%
Rename %UserProfile%\Documents\aBiPBackupTool\aBiPArchives\Today ar-poc-tenants_"%todaysDate%"
echo Renamed Folder