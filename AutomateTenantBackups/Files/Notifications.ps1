$mydocuments = [environment]::getfolderpath(“mydocuments”)
$mylogo = “$mydocuments\aBiPBackupTool\logo.png”

New-BurntToastNotification -AppLogo $mylogo -Text "S3 Glacier Vault",
                                                           'Backup has completed successfully!'