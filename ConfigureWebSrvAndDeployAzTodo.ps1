# This script configures a Windows 2016 Datacenter VM to run AzTodo:
# 1. Enables IIS with necessary features
# 2. Installs ASP.NET Core
# 4. Deploys AzTodo
# 5. Opens ports 80
param(

    [Parameter(Mandatory=$True)]
    [string]$ServerEndpoint,

    [Parameter(Mandatory=$False)]
    [string]$SqlUserName = "aztodosqluser",

    [Parameter(Mandatory=$True)]
    [string]$SqlPwd
)

function Create-ASPStateDB
{
    try
    {
        $aspNetRegSqlPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regsql.exe"
        if(-not (Test-Path -Path $aspNetRegSqlPath))
        {
            throw "aspnet_regsql.exe not found."
        }
        Start-Process -FilePath $aspNetRegSqlPath -ArgumentList "-S $ServerEndpoint -U $SqlUserName -P $SqlPwd -ssadd -sstype p"
        
        Write-Host "Created ASPState DB successfully."
    }
    catch [Exception]
    {
        Write-Host "Creating ASPState DB failed with exception:" $_.Exception.GetType().FullName
        Write-Host $_.Exception.Message
        exit 1
    }
}

function Install-AspNetCore
{
	$webClient = New-Object -TypeName System.Net.WebClient
	$destination= "C:\WindowsAzure\DotNetCore.2.0.0-WindowsHosting.exe"
	$webClient.DownloadFile("https://aka.ms/dotnetcore.2.0.0-windowshosting",$destination)
	Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force
	Start-Process -FilePath "C:\WindowsAzure\DotNetCore.2.0.0-WindowsHosting.exe" -ArgumentList "/quiet" -Wait
	Invoke-Command -scriptblock {iisreset}
}

function Deploy-AzTodo
{
    try
    {
        Add-Type -AssemblyName System.IO.Compression.FileSystem

        # Get zip file and unpack
        $blobName = "AzTodo.zip"
        $storageAccountName = "azurestackbetastorage"
        $container = "aztodo"
        $outpath = "$env:temp\AzTodo"
        $context = New-AzureStorageContext -StorageAccountName $storageAccountName -Anonymous
        Get-AzureStorageBlobContent -Context $context -Container $container -Blob $blobName -Destination $env:temp -Force
        [System.IO.Compression.ZipFile]::ExtractToDirectory("$env:temp\$blobName", $outpath)

        $defaultConnString = "Server=tcp:$ServerEndpoint,1433;Integrated Security=false;User ID=$SqlUserName;Password=$SqlPwd;Database=AzTodo" 
        #$sessionConnString = "Server=tcp:$ServerEndpoint,1433;Integrated Security=false;User ID=$SqlUserName;Password=$SqlPwd;Database=ASPState"
        $appSettingsJson = Get-Content $outpath\appsettings.json -Raw | ConvertFrom-Json
        $appSettingsJson.ConnectionStrings.DefaultConnection = $defaultConnString
        $appSettingsJson | ConvertTo-Json | Set-Content $outpath\appsettings.json

        # Copy contents to C:\inetpub\wwwroot
        Copy-Item $env:temp\AzTodo\* C:\inetpub\wwwroot -Recurse -Force
        
        Write-Host "Deployed AzTodo successfully."
    }
        catch [Exception]
    {
        Write-Host "Deploying AzTodo failed with exception:" $_.Exception.GetType().FullName
        Write-Host $_.Exception.Message
        exit 1
    }
}

# AzureRM module functions are needed for accessing blob storage.
try
{ 
    $azureRmModule = Get-Module | Where-Object { $_.Name -eq "AzureRM" }
    if($azureRmModule -eq $null)
    {
        Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
        Install-Module AzureRM -Force
        Import-Module AzureRM
    }

    Write-Host "AzureRM module installed successfully."
}
catch [Exception]
{
    Write-Host "Installing AzureRM module failed with exception:" $_.Exception.GetType().FullName
    Write-Host $_.Exception.Message
    exit 1
}

try
{
    # Enable IIS-WebServerRole and ASPNET45
    if((Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole).State -eq "Disabled")
    {
        Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All
    }

    <#if((Get-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45).State -eq "Disabled")
    {
        Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45 -All
    }#>
    
    Write-Host "IIS enabled successfully."
}
catch [Exception]
{
    Write-Host "Enabling IIS features failed with exception:" $_.Exception.GetType().FullName
    Write-Host $_.Exception.Message
    exit 1
}

# Finish AzTodo prerequisites and deploy AzTodo
#Create-ASPStateDB
Install-AspNetCore
Deploy-AzTodo

try
{
    # Open ports 80
    netsh advfirewall firewall add rule name="Open port 80" dir=in action=allow protocol=TCP localport=80
    
    Write-Host "Ports 80 opened successfully."
}
catch [Exception]
{
    Write-Host "Open portsfailed with exception:" $_.Exception.GetType().FullName
    Write-Host $_.Exception.Message
    exit 1
}