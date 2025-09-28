# Dockerfile para backend C# .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["backend.csproj", "./"]
RUN dotnet restore "./backend.csproj"  # Restaura dependencias primero
COPY . .
RUN dotnet build "backend.csproj" -c Release -o /app/build  # Compila en Release

FROM build AS publish
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false  # Publica sin host nativo

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]