#!/bin/sh

# Build web site
docker run -t --rm \
    -v $(PWD):/app --workdir /app \
    microsoft/aspnetcore-build \
    bash -c "cd ./TooYoung.Web && dotnet restore && dotnet publish -c Release -o ./bin/Release/PublishOutput"

# Build test
cd ../Test
docker run -t --rm \
    -v $(PWD):/app --workdir /app \
    microsoft/aspnetcore-build \
    bash -c "cd ./Test && dotnet restore && dotnet test"