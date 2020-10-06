FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish XCloud.GetPeers.Api -c Release -o XCloud.GetPeers.Api/out
# Build runtime image

FROM base AS final
COPY --from=build /app/XCloud.GetPeers.Api/out .

RUN mkdir -p config

RUN ["chmod", "+x", "startgetpeers.sh"]

COPY XCloud.GetPeers.Api/startgetpeers.sh /usr/local/bin/
ENTRYPOINT ["startgetpeers.sh"]