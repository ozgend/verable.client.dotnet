FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Sample/Service1/Service1.csproj", "Sample/Service1/"]
RUN dotnet restore "Sample/Service1/Service1.csproj"
COPY . .
WORKDIR "/src/Sample/Service1"
RUN dotnet build "Service1.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Service1.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Service1.dll"]
