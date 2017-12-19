#!/bin/sh

# Build web site
docker run -t --rm \
    -v $(pwd):/app --workdir /app \
    microsoft/aspnetcore-build \
    ba -c "cd ./TooYoung.Web && dotnet restore && dotnet publish -c Release -o ./bin/Release/PublishOutput"

# Build test
cd ../Test
docker run -t --rm \
    -v $(PWD):/app --workdir /app \
    microsoft/aspnetcore-build \
    sh -c "cd ./Test && dotnet restore && dotnet test"