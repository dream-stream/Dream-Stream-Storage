FROM dreamstream/dotnetcore:3.0.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /src
COPY ["Dream-Stream-Storage/Dream-Stream-Storage.csproj", "Dream-Stream-Storage/"]
RUN dotnet restore -r linux-arm "Dream-Stream-Storage/Dream-Stream-Storage.csproj"
COPY . .
WORKDIR "/src/Dream-Stream-Storage"
RUN dotnet build "Dream-Stream-Storage.csproj" -r linux-arm -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dream-Stream-Storage.csproj" -r linux-arm -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dream-Stream-Storage.dll"]