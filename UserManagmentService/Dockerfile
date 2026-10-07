FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj файлы (пути от корня D:\InnoShop)
# Обратите внимание: имя папки / имя файла.csproj
COPY ["UserManagmentService/UserService.Api.csproj", "UserManagmentService/"]
COPY ["UserService.Application/UserService.Service.csproj", "UserService.Application/"]
COPY ["UserService.Domain/UserService.Domain.csproj", "UserService.Domain/"]
COPY ["UserService.Infrastructure/UserService.Infrastructure.Presintation.csproj", "UserService.Infrastructure/"]
COPY ["UserService.Infrastructure.Persistence/UserService.Infrastructure.Persistence.csproj", "UserService.Infrastructure.Persistence/"]
COPY ["UserService.Service.Abstraction/UserService.Service.Abstraction.csproj", "UserService.Service.Abstraction/"]

# Восстанавливаем пакеты
RUN dotnet restore "UserManagmentService/UserService.Api.csproj"

# Копируем весь исходный код
COPY . .

# Собираем проект
WORKDIR "/src/UserManagmentService"
RUN dotnet build "UserService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]