#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DccParser/DccParser.csproj", "DccParser/"]
RUN dotnet restore "DccParser/DccParser.csproj"
COPY . .
WORKDIR "/src/DccParser"
RUN dotnet build "DccParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DccParser.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DccParser.dll"]