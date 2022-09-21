#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /
COPY ["nuget.config", "."]
COPY ["Utg.LegalService.API/Utg.LegalService.API.csproj", "Utg.LegalService.API/"]
COPY ["Utg.LegalService.BL/Utg.LegalService.BL.csproj", "Utg.LegalService.BL/"]
COPY ["Utg.LegalService.Common/Utg.LegalService.Common.csproj", "Utg.LegalService.Common/"]
COPY ["Utg.LegalService.Dal/Utg.LegalService.Dal.csproj", "Utg.LegalService.Dal/"]
RUN dotnet restore "Utg.LegalService.API/Utg.LegalService.API.csproj"
COPY . .
WORKDIR "/Utg.LegalService.API"
RUN dotnet build "Utg.LegalService.API.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "Utg.LegalService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utg.LegalService.API.dll"]