#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /
COPY ["nuget.config", "."]
COPY ["Utg.AuditService.API/Utg.AuditService.API.csproj", "Utg.AuditService.API/"]
COPY ["Utg.AuditService.BL/Utg.AuditService.BL.csproj", "Utg.AuditService.BL/"]
COPY ["Utg.AuditService.Common/Utg.AuditService.Common.csproj", "Utg.AuditService.Common/"]
COPY ["Utg.AuditService.Dal/Utg.AuditService.Dal.csproj", "Utg.AuditService.Dal/"]
RUN dotnet restore "Utg.AuditService.API/Utg.AuditService.API.csproj"
COPY . .
WORKDIR "/Utg.AuditService.API"
RUN dotnet build "Utg.AuditService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Utg.AuditService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utg.AuditService.API.dll"]