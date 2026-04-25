# Etapa de compilacion
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos archivos de solucion y proyectos
COPY TecnoFuturo.sln .
COPY TecnoFuturo.Core/TecnoFuturo.Core.csproj TecnoFuturo.Core/
COPY TecnoFuturo.Console/TecnoFuturo.Console.csproj TecnoFuturo.Console/
COPY TecnoFuturo.InMemory/TecnoFuturo.InMemory.csproj TecnoFuturo.InMemory/
COPY TecnoFuturo.Data/TecnoFuturo.Data.csproj TecnoFuturo.Data/

# Restauramos dependencias
RUN dotnet restore TecnoFuturo.sln

# Copiamos el resto del codigo
COPY . .

# Publicamos la app de consola
RUN dotnet publish TecnoFuturo.Console/TecnoFuturo.Console.csproj -c Release -o /app/publish

# Etapa final de ejecucion
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# Copiamos la publicacion
COPY --from=build /app/publish .

# Creamos la carpeta de datos dentro del contenedor
RUN mkdir -p /app/Datos

# Ejecutamos la aplicacion
ENTRYPOINT ["dotnet", "TecnoFuturo.Console.dll"]