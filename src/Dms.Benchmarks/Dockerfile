FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Dms.Benchmarks/Dms.Benchmarks.csproj", "Dms.Benchmarks/"]
RUN dotnet restore "src/Dms.Benchmarks/Dms.Benchmarks.csproj"
COPY . .
WORKDIR "/src/Dms.Benchmarks"
RUN dotnet build "Dms.Benchmarks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dms.Benchmarks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dms.Benchmarks.dll"]
