#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TipBot/Blockcore.TipBot.csproj", "TipBot/"]
RUN dotnet restore "TipBot/Blockcore.TipBot.csproj"
COPY . .
WORKDIR "/src/TipBot"
RUN dotnet build "Blockcore.TipBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blockcore.TipBot.csproj" -c Release -o /app/publish

FROM base AS final
# RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blockcore.TipBot.dll"]