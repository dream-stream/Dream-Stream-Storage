FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /src
COPY ["Dream-Stream-Storage/Dream-Stream-Storage.csproj", "Dream-Stream-Storage/"]
RUN dotnet restore "Dream-Stream-Storage/Dream-Stream-Storage.csproj"
COPY . .
WORKDIR "/src/Dream-Stream-Storage"
RUN dotnet build "Dream-Stream-Storage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dream-Stream-Storage.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dream-Stream-Storage.dll"]