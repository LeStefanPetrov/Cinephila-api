FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Cinephila.API/Cinephila.API.csproj", "src/Cinephila.API/"]
RUN dotnet restore "src\Cinephila.API\Cinephila.API.csproj"
COPY . .
WORKDIR "/src/src/Cinephila.API"
RUN dotnet build "Cinephila.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cinephila.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80 2222
COPY entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
ENTRYPOINT ["dotnet", "Cinephila.API.dll"]
