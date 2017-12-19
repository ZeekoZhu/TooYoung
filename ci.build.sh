#!/bin/sh

# Build web site
docker run -t --rm \
    -v $(pwd):/app --workdir /app \
    microsoft/aspnetcore-build \
    sh -c "cd ./TooYoung.Web && dotnet restore --configfile ../Nuget.Config && dotnet publish -c Release -o ./bin/Release/PublishOutput"

# Build test
docker run -t --rm \
    -v $(pwd):/app --workdir /app \
    microsoft/aspnetcore-build \
    sh -c "cd ./Test && dotnet restore --configfile ../Nuget.Config && dotnet test"