#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release

RUN echo "Target: $TARGETARCH"
RUN echo "Configuration: $BUILD_CONFIGURATION"

WORKDIR /src
COPY ["ConsoleApp/ConsoleApp.csproj", "ConsoleApp/"]
COPY ["Directory.*.props", "."]
RUN dotnet restore "ConsoleApp/ConsoleApp.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/ConsoleApp"
RUN dotnet build "ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]