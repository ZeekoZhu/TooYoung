FROM microsoft/aspnetcore:2.0.4-stretch

ENV NODE_VERSION 9.3.0

RUN curl -SLO "https://mirrors.ustc.edu.cn/node/v$NODE_VERSION/node-v$NODE_VERSION-linux-amd64.tar.gz" \
  && curl -SLO --compressed "https://mirrors.ustc.edu.cn/node/v$NODE_VERSION/SHASUMS256.txt" \
  && grep " node-v$NODE_VERSION-linux-amd64.tar.gz\$" SHASUMS256.txt | sha256sum -c - \
  && tar -xf "node-v$NODE_VERSION-linux-amd64.tar.gz" -C /usr/local --strip-components=1 --no-same-owner \
  && rm "node-v$NODE_VERSION-linux-amd64.tar.gz" SHASUMS256.txt \
  && ln -s /usr/local/bin/node /usr/local/bin/nodejs

WORKDIR /app
COPY ./bin/Release/netcoreapp2.0/publish .
ENTRYPOINT ["dotnet", "TooYoung.dll"]