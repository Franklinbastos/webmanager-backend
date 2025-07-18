# Use a imagem base do SDK do .NET para construir a aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia o arquivo de projeto e restaura as dependências
COPY backend/*.csproj ./backend/
WORKDIR /app/backend
RUN dotnet restore

# Copia o restante dos arquivos da aplicação
COPY . /app/
WORKDIR /app/backend

# Publica a aplicação
RUN dotnet publish -c Release -o out

# Use a imagem de tempo de execução do .NET para executar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/backend/out .

# Expõe a porta que a aplicação ASP.NET Core escuta
EXPOSE 8080

# Define o ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "backend.dll"]
