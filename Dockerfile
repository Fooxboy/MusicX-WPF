# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

COPY SignalR.Protobuf/SignalR.Protobuf.csproj SignalR.Protobuf/ 
RUN dotnet restore SignalR.Protobuf/SignalR.Protobuf.csproj -r linux-musl-x64

COPY MusicX.Shared/MusicX.Shared.csproj MusicX.Shared/ 
RUN dotnet restore MusicX.Shared/MusicX.Shared.csproj -r linux-musl-x64

COPY MusicX.Server/MusicX.Server.csproj MusicX.Server/
RUN dotnet restore MusicX.Server/MusicX.Server.csproj -r linux-musl-x64

COPY . .

WORKDIR /source/MusicX.Server
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-amd64
WORKDIR /app
COPY --from=build /app ./

# See: https://github.com/dotnet/announcements/issues/20
# Uncomment to enable globalization APIs (or delete)
# ENV \
#     DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
#     LC_ALL=en_US.UTF-8 \
#     LANG=en_US.UTF-8
# RUN apk add --no-cache icu-libs

RUN mkdir files

ENTRYPOINT ["./MusicX.Server"]