# Stage 1 Build and test
FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY Test/*.csproj ./Test/
COPY TooYoung.Web/*.csproj ./TooYoung.Web/
COPY Nuget.Config .
RUN ls -R && cd Test \
    && dotnet restore --configfile ../Nuget.Config \
    && cd ../TooYoung.Web \
    && dotnet restore --configfile ../Nuget.Config

# copies the rest of your code
COPY . .
RUN cd Test && dotnet test && cd ../TooYoung.Web \
    && dotnet publish -c Release -o /app/

# Stage 2 Build image
FROM microsoft/aspnetcore:2.0.4-stretch

ENV NODE_VERSION 9.3.0

RUN curl -SLO "https://mirrors.ustc.edu.cn/node/v$NODE_VERSION/node-v$NODE_VERSION-linux-x64.tar.gz" \
  && curl -SLO --compressed "https://mirrors.ustc.edu.cn/node/v$NODE_VERSION/SHASUMS256.txt" \
  && grep " node-v$NODE_VERSION-linux-x64.tar.gz\$" SHASUMS256.txt | sha256sum -c - \
  && tar -xf "node-v$NODE_VERSION-linux-x64.tar.gz" -C /usr/local --strip-components=1 --no-same-owner \
  && rm "node-v$NODE_VERSION-linux-x64.tar.gz" SHASUMS256.txt \
  && ln -s /usr/local/bin/node /usr/local/bin/nodejs

WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "TooYoung.dll"]