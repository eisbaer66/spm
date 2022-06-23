# # Run a separate PowerShell process because the script calls exit, so it will end the current PowerShell session.
# &powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Runtime dotnet"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

Function Get-InstalledDotnetCoreVersion
{
    [CmdletBinding()]
    [OutputType([PSObject])]
    param()

    [Hashtable] $SDKVersions = [ordered] @{}
    [Hashtable] $RuntimeVersions = [ordered] @{}
    [Hashtable] $FrameworkHostingVersions = [ordered] @{}
    [Hashtable] $ServerHostingBundleVersions = [ordered] @{}
    [Hashtable] $AspNetRuntimePackageStoreVersions = [ordered] @{}

    try
    {
        [string] $RootDotnetPath = $PSVersionTable.Platform -eq "Unix" ? "/usr/share/dotnet" : (Join-Path -Path $env:programfiles -ChildPath 'dotnet')
        [string] $SdkPath = Join-Path -Path $RootDotnetPath -ChildPath 'sdk'
        [string] $RuntimePath = Join-Path -Path $RootDotnetPath -ChildPath 'shared\Microsoft.NETCore.App'
        [string] $SFHPath = Join-Path -Path $RootDotnetPath -ChildPath 'host\fxr'

        if (Test-Path -Path $RootDotnetPath -PathType Container)
        {
            if (Test-Path -Path $SdkPath -PathType Container)
            {
                Write-Verbose "$($SdkPath) was found. Enumerating versions now."

                Get-ChildItem -Path $SdkPath -Directory | Where-Object { $_.Name -match '^\d.\d.\d' } `
                    | Sort-Object -Property Name | Foreach-Object { $SDKVersions.Add( $_.Name, $_.FullName ) }
            }

            if (Test-Path -Path $RuntimePath -PathType Container)
            {
                Write-Verbose "$($RuntimePath) was found. Enumerating versions now."

                Get-ChildItem -Path $RuntimePath -Directory | Where-Object { $_.Name -match '^\d.\d.\d' } `
                    | Sort-Object -Property Name | Foreach-Object { $RuntimeVersions.Add( $_.Name, $_.FullName ) }
            }

            if (Test-Path -Path $SFHPath -PathType Container)
            {
                Write-Verbose "$($SFHPath) was found. Enumerating versions now."

                Get-ChildItem -Path $SFHPath -Directory | Where-Object { $_ -match '^\d.\d.\d' } `
                    | Sort-Object -Property Name | Foreach-Object { $FrameworkHostingVersions.Add( $_.Name, $_.FullName ) }
            }
        }

        $ServerHostingBundleVersions = Get-InstalledDotnetCoreBundles -BundleName 'WindowsServerHosting'
        $AspNetRuntimePackageStoreVersions = Get-InstalledDotnetCoreBundles -BundleName 'ASPNetRuntimePackageStore'
        
        [Hashtable] $Properties = @{ 'SDKVersions' = $SDKVersions;
            'RuntimeVersions' = $RuntimeVersions;
            'SharedFrameworkHostingVersions' = $FrameworkHostingVersions;
            'WindowsServerHostingVersions' = $ServerHostingBundleVersions;
            'AspNetRuntimePackageStoreVersions' = $AspNetRuntimePackageStoreVersions
        }

        $Result = New-Object -TypeName PSObject -Prop $Properties

        return $Result
    }
    catch
    {
        throw $_
    }
}


Function Get-InstalledDotnetCoreBundles
{
    [CmdletBinding()]
    [OutputType([HashTable])]
    param(
        [Parameter(Mandatory = $true, HelpMessage = 'DotnetCore bundle name to search for')]
        [ValidateSet('WindowsServerHosting', 'ASPNetRuntimePackageStore')]
        [string]
        $BundleName
    )

    [HashTable] $InstalledVersion = [ordered] @{}
    [string] $RootPath = 'HKLM:\SOFTWARE\Wow6432Node\Microsoft\Updates\.NET Core'
    [string] $PathFilter = ''

    try
    {
        switch ($BundleName)
        {
            'WindowsServerHosting'  { $PathFilter = 'Microsoft .Net Core*Windows Server Hosting' }
            'ASPNetRuntimePackageStore' { $PathFilter = 'Microsoft ASP.NET Core*Runtime Package Store*' }
        }

        if (Test-Path -Path $RootPath -PathType Container)
        {
            [string[]] $InstalledSHBundles = Get-ChildItem -Path $RootPath `
                | Where-Object { $_.PSChildName -like $PathFilter } `
                | Sort-Object -Property PSChildName | Select-Object -ExpandProperty PSChildName
            
            for ($i = 0; $i -lt $InstalledSHBundles.Count; $i++)
            {
                if ($InstalledSHBundles[$i] -match '\d.\d.\d')
                {
                    $InstalledVersion.Add($Matches[0], $InstalledSHBundles[$i])
                }
            }
        }
    }
    catch
    {
        throw $_
    }
    finally
    {
        Write-Output -InputObject $InstalledVersion -NoEnumerate
    }
}


Function Assert-InstalledDotnetCoreRuntimeVersion
{
    [CmdletBinding()]
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true, HelpMessage = 'DotnetCore version to search for')]
        [string]
        $Version
    )
    
    Add-Type -AssemblyName System.Runtime
    $versions = Get-InstalledDotnetCoreVersion
    $expectedVersion = $null
    if (-not [System.Version]::TryParse($Version,([ref]$expectedVersion)))
    {
        throw "could not parse expected dotnet version '$Version'"
    }
    
    $matchingVersions = $versions.RuntimeVersions.Keys  | 
        ForEach-Object {
            $out = $null
    
            if (-not [System.Version]::TryParse($_,([ref]$out)))
            {
                throw "could not parse installed dotnet version '$_'"
            }
            $out
        } |
        Where-Object {
            $_ -ge $expectedVersion
        }
    
    if ($matchingVersions.Count -eq 0)
    {
        throw "No matching dotnet runtime found on the machine. Goto https://dotnet.microsoft.com/en-us/download and 'Download .NET Runtime'"
    }
}

Function Install-SourcePawnManager
{
    [CmdletBinding()]
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true, HelpMessage = 'SourcePawnManager version to install')]
        [string]
        $Version,

        [Parameter(Mandatory = $false, HelpMessage = 'path SourcePawnManager gets installed in')]
        [string]
        $DestinationPath = $pwd.Path
    )

    $filename = $PSVersionTable.Platform -eq "Unix" ? "linux-x64.zip" : "win-x64.zip"
    $url = "https://github.com/eisbaer66/spm/releases/download/$Version/$filename"
    $url
    Invoke-WebRequest -Uri $url -OutFile $filename

    Expand-Archive $filename -Force -DestinationPath $DestinationPath

    Remove-Item -Path $filename
}

Function Add-Path
{
    [CmdletBinding()]
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $false, HelpMessage = 'path to add to the PATH')]
        [string]
        $Path = $pwd.Path
    )
    
    $envPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($envPath -like "*$Path*")
    {
        return
    }

    [Environment]::SetEnvironmentVariable("Path", $envPath + ";$Path", "User")
}

$requiredDotnetRuntimeVersion = "6.0"
$spmVersion = "v0.1"
Assert-InstalledDotnetCoreRuntimeVersion -Version $requiredDotnetRuntimeVersion
Install-SourcePawnManager -Version $spmVersion
Add-Path

Write-Output "spm $spmVersion installed and added '$($pwd.Path)' to your PATH"
