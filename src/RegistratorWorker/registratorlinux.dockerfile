﻿FROM microsoft/aspnetcore-build:2.0.3-stretch AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0.3-stretch
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "RegistratorWorker.dll"]