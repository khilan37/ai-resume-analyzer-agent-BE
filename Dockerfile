FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY SmartResumeAnalyzerAgent.sln ./
COPY Directory.Build.props ./
COPY src/SmartResumeAnalyzerAgent.Api/SmartResumeAnalyzerAgent.Api.csproj src/SmartResumeAnalyzerAgent.Api/
COPY src/SmartResumeAnalyzerAgent.Application/SmartResumeAnalyzerAgent.Application.csproj src/SmartResumeAnalyzerAgent.Application/
COPY src/SmartResumeAnalyzerAgent.Domain/SmartResumeAnalyzerAgent.Domain.csproj src/SmartResumeAnalyzerAgent.Domain/
COPY src/SmartResumeAnalyzerAgent.Infrastructure/SmartResumeAnalyzerAgent.Infrastructure.csproj src/SmartResumeAnalyzerAgent.Infrastructure/

RUN dotnet restore SmartResumeAnalyzerAgent.sln

COPY . .

RUN dotnet publish src/SmartResumeAnalyzerAgent.Api/SmartResumeAnalyzerAgent.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SmartResumeAnalyzerAgent.Api.dll"]
