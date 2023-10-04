FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY bin/Release/net7.0/publish/. .
ENTRYPOINT ["dotnet", "EDP.Monitor.ElkAlert.dll"]