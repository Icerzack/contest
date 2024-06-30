# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish Camelot.Api.sln -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://*:5001

ENV DB_CONNECTION="Host=camelotdb;Port=5432;Database=camelot_db;Username=admin;Password=admin;Timeout=15;Trust Server Certificate=true;"
ENV JWT_KEY="This is my very Secret Key for the api"
ENV SOCKET_KEY="This is my very Secret Key for the api"
CMD dotnet Camelot.Api.dll
