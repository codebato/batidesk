FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/WestDesk.API/WestDesk.API.csproj", "src/WestDesk.API/"]
COPY ["src/WestDesk.Application/WestDesk.Application.csproj", "src/WestDesk.Application/"]
COPY ["src/WestDesk.Domain/WestDesk.Domain.csproj", "src/WestDesk.Domain/"]
COPY ["src/WestDesk.Infrastructure/WestDesk.Infrastructure.csproj", "src/WestDesk.Infrastructure/"]
RUN dotnet restore "src/WestDesk.API/WestDesk.API.csproj"

COPY . .
WORKDIR "/src/src/WestDesk.API"
RUN dotnet build "WestDesk.API.csproj" -c Release -o /app/build

RUN dotnet publish "WestDesk.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WestDesk.API.dll"]
