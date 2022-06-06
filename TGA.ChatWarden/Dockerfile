#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TGA.ChatWarden/TGA.ChatWarden.csproj", "TGA.ChatWarden/"]
COPY ["TGA.CoreLib/TGA.CoreLib.csproj", "TGA.CoreLib/"]
RUN dotnet restore "TGA.ChatWarden/TGA.ChatWarden.csproj"
COPY . .
WORKDIR "/src/TGA.ChatWarden"
RUN dotnet build "TGA.ChatWarden.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TGA.ChatWarden.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGA.ChatWarden.dll"]