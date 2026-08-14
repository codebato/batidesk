FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/NvnDesk.API/NvnDesk.API.csproj", "src/NvnDesk.API/"]
COPY ["src/NvnDesk.Application/NvnDesk.Application.csproj", "src/NvnDesk.Application/"]
COPY ["src/NvnDesk.Domain/NvnDesk.Domain.csproj", "src/NvnDesk.Domain/"]
COPY ["src/NvnDesk.Infrastructure/NvnDesk.Infrastructure.csproj", "src/NvnDesk.Infrastructure/"]
RUN dotnet restore "src/NvnDesk.API/NvnDesk.API.csproj"

COPY . .
WORKDIR "/src/src/NvnDesk.API"
RUN dotnet build "NvnDesk.API.csproj" -c Release -o /app/build

RUN dotnet publish "NvnDesk.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NvnDesk.API.dll"]
