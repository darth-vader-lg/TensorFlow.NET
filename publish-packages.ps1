[CmdletBinding(PositionalBinding=$false)]
Param(
  [string][Alias('c')]$configuration = "Release",
  [string][Alias('k', "api-key")]$apiKey,
  [string]$platform = $null,
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

# Check for the api key consistence
if (($apiKey -eq "") -and (-not (Test-Path env:GITHUB_NUPKG_PUSH_KEY) -or $env:GITHUB_NUPKG_PUSH_KEY -eq "")) {
    Write-Error "Push apy-key not specified!"
    Write-Error "Use the -api-key switch or define the environment variable GITHUB_NUPKG_PUSH_KEY"
    exit 1
}
if ($apiKey -eq "") {
    $apiKey = $env:GITHUB_NUPKG_PUSH_KEY
}

# List of created packages in the selected configuration and platform
if ($platform -ne "") {
    $platformDir = $platform, $configuration -join "."
}
else {
    $platformDir = $configuration
}

# List of projects and packages
$packages = Get-ChildItem -Path src -Filter "*.nupkg" -Recurse | Where-Object { $_.Directory.Name -eq "Release" }

# GitHub package destination
$pushOrigin = (((git remote -v) -match "(push)") -split '\t')[0]
$pushUrl = git remote get-url --push $pushOrigin
$user = Split-Path -leaf (Split-Path $pushUrl)
$source = "https://nuget.pkg.github.com/", $user, "/index.json" -join ''

# Push the packages
foreach ($package in $packages) {
    # Workaround for the timeout; it must not be a multiple of 60, otherwise it will be ignored
    dotnet nuget push $package.FullName --force-english-output --skip-duplicate --no-symbols true --timeout 3601 --source $source --api-key $apiKey
}
