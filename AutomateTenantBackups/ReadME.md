# AWS S3 Glacier vault backups

## About
This app helps to automate backups from AWS S3 buckets to a specified Vault.
The basic flow of this program allows users to :
1. Have a designated S3 bucket in any region
2. Copy the content of the S3 bucket without modifying the object class of stored items to a specified vault
3. Once a backup has successfully been uploaded to a vault, a Windows Tray Notifcation will appear


## Installation
1. Run Glacier Setup.exe as Administrator to install missing dependencies and to setup relevant aws configs
2. Run .exe on a scheduler depending on backup frequency

## License
Author Umer Raja
[MIT](https://choosealicense.com/licenses/mit/)