# syntax=docker/dockerfile:1.7
# Multi-stage, multi-arch capable build. BuildKit/buildx fills TARGETARCH at build time.
ARG DOTNET_VERSION=8.0
ARG TARGETARCH

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG TARGETARCH
WORKDIR /src

COPY RouterOSMCPSharp.csproj ./
RUN dotnet restore RouterOSMCPSharp.csproj -a $TARGETARCH

COPY . .
RUN dotnet publish RouterOSMCPSharp.csproj \
        -c Release \
        -a $TARGETARCH \
        --no-restore \
        -o /app/publish \
        /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Logs directory persists outside the image when the user mounts a volume.
RUN mkdir -p /app/logs

COPY --from=build /app/publish ./

ENV DOTNET_ENVIRONMENT=Production \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    Server__Host=0.0.0.0 \
    Server__Port=5100

EXPOSE 5100
VOLUME ["/app/logs"]

ENTRYPOINT ["dotnet", "RouterOSMCPSharp.dll"]
