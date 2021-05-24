[CmdletBinding(PositionalBinding=$false)]
Param(
    [string][Alias('c')]$configuration = "Release",
    [string]$platform = $null,
    [string] $projects,
    [string][Alias('v')]$verbosity = "minimal",
    [bool] $warnAsError = $false,
    [bool] $nodeReuse = $true,
    [switch][Alias('r')]$restore,
    [switch][Alias('b')]$build,
    [switch] $rebuild,
    [switch] $deploy,
    [switch] $sign,
    [switch] $clean,
    [string] $runtimeSourceFeed = '',
    [string] $runtimeSourceFeedKey = '',
    [switch] $help,
    [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

# Unset 'Platform' environment variable to avoid unwanted collision in InstallDotNetCore.targets file
# some computer has this env var defined (e.g. Some HP)
if($env:Platform) {
    $env:Platform=""  
}

function Print-Usage() {
    Write-Host "Common settings:"
    Write-Host "  -configuration <value>  Build configuration: 'Debug' or 'Release' (short: -c)"
    Write-Host "  -platform <value>       Platform configuration: 'x86', 'x64' or any valid Platform value to pass to msbuild"
    Write-Host "  -verbosity <value>      Msbuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic] (short: -v)"
    Write-Host "  -help                   Print help and exit"
    Write-Host ""

    Write-Host "Actions:"
    Write-Host "  -restore                Restore dependencies (short: -r)"
    Write-Host "  -build                  Build solution (short: -b)"
    Write-Host "  -rebuild                Rebuild solution"
    Write-Host "  -deploy                 Deploy built VSIXes"
    Write-Host "  -sign                   Sign build outputs"
    Write-Host "  -clean                  Clean the solution"
    Write-Host ""

    Write-Host "Advanced settings:"
    Write-Host "  -projects <value>       Semi-colon delimited list of sln/proj's to build. Globbing is supported (*.sln)"
    Write-Host "  -warnAsError <value>    Sets warnaserror msbuild parameter ('true' or 'false')"
    Write-Host ""

    Write-Host "Command line arguments not listed above are passed thru to msbuild."
    Write-Host "The above arguments can be shortened as much as to be unambiguous (e.g. -co for configuration, -t for test, etc.)."
}


function Build {
    $bl = if ($binaryLog) { '/bl:' + (Join-Path $LogDir 'Build.binlog') } else { '' }
    $platformArg = if ($platform) { "/p:Platform=$platform" } else { '' }

    if ($projects) {
        # Re-assign properties to a new variable because PowerShell doesn't let us append properties directly for unclear reasons.
        # Explicitly set the type as string[] because otherwise PowerShell would make this char[] if $properties is empty.
        [string[]] $msbuildArgs = $properties
        
        # Resolve relative project paths into full paths 
        $projects = ($projects.Split(';').ForEach({Resolve-Path $_}) -join ';')
        
        $msbuildArgs += "/p:Projects=$projects"
        $properties = $msbuildArgs
    }

    dotnet build `
        $bl `
        $platformArg `
        /p:Configuration=$configuration `
        /p:RepoRoot=$RepoRoot `
        /p:Restore=$restore `
        /p:Build=$build `
        /p:Rebuild=$rebuild `
        /p:Deploy=$deploy `
        /p:Sign=$sign `
        @properties
}

try {

    if ($help -or (($null -ne $properties) -and ($properties.Contains('/help') -or $properties.Contains('/?')))) {
        Print-Usage
        exit 0
    }

    if ($clean -or $rebuild) {
        Get-ChildItem src\* -Include *.nupkg -Recurse | Remove-Item
        dotnet clean -c $configuration
        if ($clean) { exit 0 }
    }

    dotnet pack -c $configuration .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist.nupkgproj
    dotnet pack -c $configuration .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist-Windows-GPU.nupkgproj
    Build
    exit 0
}
catch {
    Write-Host $_.ScriptStackTrace
    exit 1
}
