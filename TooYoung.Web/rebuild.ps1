#!/bin/pwsh

$outDir = './wwwroot/dist', './obj', './bin', './ClientApp/dist'

foreach ($dir in $outDir) {
    if (Test-Path -Path $dir) {
        Remove-Item -Path $dir -Recurse -Force;
    }
}

dotnet build
