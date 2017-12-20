#!/bin/pwsh
$Env:ASPNETCORE_ENVIRONMENT = 'Development'
$Env:ASPNETCORE_URLS = 'http://localhost:53148'
dotnet watch run
