FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src

COPY Directory.Build.props ./
COPY apps/api/*.csproj ./app/
WORKDIR /src/app
RUN dotnet restore

COPY apps/api/. .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY apps/api/*.csproj ./app/
WORKDIR /src/app
RUN dotnet restore

COPY apps/api/. .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Mooch.Api.dll"]
