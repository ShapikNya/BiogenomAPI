FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Biogenom.WebApi/Biogenom.WebApi.csproj Biogenom.WebApi/
COPY Biogenom.Application/Biogenom.Application.csproj Biogenom.Application/
COPY Biogenom.Domain/Biogenom.Domain.csproj Biogenom.Domain/
COPY Biogenom.Persistence/Biogenom.Persistence.csproj Biogenom.Persistence/
RUN dotnet restore Biogenom.WebApi/Biogenom.WebApi.csproj

COPY . .
RUN dotnet publish Biogenom.WebApi/Biogenom.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Biogenom.WebApi.dll"]
