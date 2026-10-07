FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj файлы (пути от корня D:\InnoShop)
COPY ["ProductManagmentService/ProductService.Api.csproj", "ProductManagmentService/"]
COPY ["ProductService.Application/ProductService.Application.csproj", "ProductService.Application/"]
COPY ["ProductService.Domain/ProductService.Domain.csproj", "ProductService.Domain/"]
COPY ["ProductService.Infrastructure/ProductService.Infrastructure.Persistence.csproj", "ProductService.Infrastructure/"]
COPY ["ProductService.Infrastructure.Presintation/ProductService.Infrastructure.Presintation.csproj", "ProductService.Infrastructure.Presintation/"]
COPY ["ProductService.Service/ProductService.Service.csproj", "ProductService.Service/"]
COPY ["ProductService.Service.Abstraction/ProductService.Service.Abstraction.csproj", "ProductService.Service.Abstraction/"]

# Восстанавливаем пакеты
RUN dotnet restore "ProductManagmentService/ProductService.Api.csproj"

# Копируем весь исходный код
COPY . .

# Собираем проект
WORKDIR "/src/ProductManagmentService"
RUN dotnet build "ProductService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]