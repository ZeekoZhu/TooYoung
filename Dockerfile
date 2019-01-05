# Stage 1 Build and test
FROM zeekozhu/aspnetcore-build-yarn:2.2 AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY src/TooYoung.Web/*.fsproj ./TooYoung.Web/
COPY Nuget.Config .
RUN cd Test \
    && dotnet restore --configfile ../Nuget.Config \
    && cd ../TooYoung.Web \
    && dotnet restore --configfile ../Nuget.Config

COPY TooYoung.Web/ClientApp/yarn.lock ./TooYoung.Web/ClientApp/
RUN ls -R && cd TooYoung.Web/ClientApp && yarn

COPY . /app_src
RUN cd /app_src \
    && dotnet tool

# copies the rest of your code
COPY . .
RUN cd Test && dotnet test \
    && cd ../TooYoung.Web \
    && dotnet publish -c Release -o /app/

# Stage 2 Build image
FROM zeekozhu/aspnetcore-node:2.2

WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "TooYoung.Web.dll"]
