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
if($env:Platform) { $env:Platform="" }

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

    if (-not($PSScriptRoot) -and $psISE) { $scriptRoot = Split-Path $psISE.CurrentFile.FullPath } else { $scriptRoot = $PSScriptRoot }

    dotnet build `
        $bl `
        $platformArg `
        /p:Configuration=$configuration `
        /p:RepoRoot=$RepoRoot `
        /p:Restore=$restore `
        /p:RestoreAdditionalProjectSources=$scriptRoot\src\SciSharp.TensorFlow.Redist\bin\$configuration `
        /p:Build=$build `
        /p:Rebuild=$rebuild `
        /p:Deploy=$deploy `
        /p:Sign=$sign `
        @properties
}

function SetVars {
    [CmdletBinding()]
    param(
        ## The path to the script to run
        [Parameter(Mandatory = $true)]
        [string] $Path,

        ## The arguments to the script
        [string] $ArgumentList
    )

    Set-StrictMode -Version 3

    $tempFile = [IO.Path]::GetTempFileName()

    ## Store the output of cmd.exe.  We also ask cmd.exe to output
    ## the environment table after the batch file completes
    #cmd /c " `"$Path`" $argumentList && set > `"$tempFile`" "
    cmd.exe /c "`"$Path`" $argumentList & set > `"$tempFile`" "

    ## Go through the environment variables in the temp file.
    ## For each of them, set the variable in our local environment.
    Get-Content $tempFile | Foreach-Object {
        if($_ -match "^(.*?)=(.*)$")
        {
            Set-Content "env:\$($matches[1])" $matches[2]
        }
    }
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
    
    # Download the Tensorflow libraries
    dotnet build /t:GetFilesFromArchive .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist.nupkgproj
    dotnet build /t:GetFilesFromArchive .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist-Windows-GPU.nupkgproj
    dotnet build /t:GetFilesFromArchive .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist-Windows-CUDA10_1-SM30.nupkgproj

    if (-not $env:VisualStudioVersion) {
        $vsWhere = ${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\Installer\vswhere.exe"
        if (Test-Path -Path $vsWhere) {
            $vsCommonTools = & $vsWhere -latest -prerelease -property installationPath
            $vsCommonTools = $vsCommonTools + "\Common7\Tools"
        }
        if (-not (Test-Path -Path $vsCommonTools)) {
            $vsCommonTools = $env:VS140COMMONTOOLS
        }
        if (-not (Test-Path -Path $vsCommonTools)) {
            Write-Error "Can't find VS 2015, 2017 or 2019"
            Write-Error "Error: Visual Studio 2015, 2017 or 2019 required"
            exit 1
        }
        $env:VSCMD_START_DIR=$PSScriptRoot
        $VsDevCmd = $vsCommonTools + "\VsDevCmd.bat"
        SetVars($VsDevCmd)
    }

    # RunVCVars
    if (-not $platform) { $__VCBuildArch = "x64" } else { $__VCBuildArch = $platform }
    if ($env:VisualStudioVersion -eq "16.0") {
        $__PlatformToolset="v142"
        $__VSVersion="16 2019"
        # SetVars -Path $cmd -ArgumentList "x64"
        SetVars -Path ($env:VS160COMNTOOLS + "..\..\VC\Auxiliary\Build\vcvarsall.bat") -ArgumentList $__VCBuildArch
    } elseif ($env:VisualStudioVersion -eq "15.0") {
        $__PlatformToolset="v141"
        $__VSVersion="15 2017"
        SetVars -Path ($env:VS150COMNTOOLS + "..\..\VC\Auxiliary\Build\vcvarsall.bat") -ArgumentList $__VCBuildArch
    } elseif ($env:VisualStudioVersion -eq "14.0") {
        $__PlatformToolset="v140"
        $__VSVersion="14 2015"
        SetVars -Path ($env:VS140COMNTOOLS + "..\..\VC\vcvarsall.bat") -ArgumentList $__VCBuildArch
    } else {
        Write-Error "Can't find VS 2015, 2017 or 2019"
        Write-Error "Error: Visual Studio 2015, 2017 or 2019 required"
        exit 1
    }

    # Build the tensorflow.dll missing function's exporter
    if (-not($PSScriptRoot) -and $psISE) { $scriptRoot = Split-Path $psISE.CurrentFile.FullPath } else { $scriptRoot = $PSScriptRoot }
    msbuild "src\TensorFlow.Exports\Tensorflow.Exports.vcxproj" /t:build /p:Configuration=$configuration /p:RestoreAdditionalProjectSources=$scriptRoot\src\SciSharp.TensorFlow.Redist\bin\$configuration
    
    # Remove variable set by Visual Studio Environment, otherwise the build will fail
    if($env:Platform) { $env:Platform="" }
    $Env:VisualStudioDir = ''
    $Env:VisualStudioEdition = ''
    $Env:VisualStudioVersion = ''

    # Pack the TensorFlow redists
    dotnet pack --no-restore -c $configuration .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist.nupkgproj
    dotnet pack --no-restore -c $configuration .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist-Windows-GPU.nupkgproj
    dotnet pack --no-restore -c $configuration .\src\SciSharp.TensorFlow.Redist\SciSharp.TensorFlow.Redist-Windows-CUDA10_1-SM30.nupkgproj

    # Build the solution
    Build

    exit 0
}
catch {
    Write-Host $_.ScriptStackTrace
    exit 1
}
