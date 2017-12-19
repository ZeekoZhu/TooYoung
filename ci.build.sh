#!/bin/sh

# Build web site
docker run -t --rm \
    -v $(pwd):/app --workdir /app \
    microsoft/aspnetcore-build \
    sh -c "cd /app/TooYoung.Web && dotnet restore --configfile /app/Nuget.Config && dotnet publish -c Release -o /app/TooYoung/PublishOutput"

# Build test
docker run -t --rm \
    -v $(pwd):/app --workdir /app \
    microsoft/aspnetcore-build \
    sh -c "cd /app/Test && dotnet restore --configfile /app/Nuget.Config && dotnet test"