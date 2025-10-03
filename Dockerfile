# ใช้ .NET 9.0 SDK สำหรับ build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj และ restore
COPY *.csproj ./
RUN dotnet restore

# copy source ทั้งหมด
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ใช้ .NET 9.0 Runtime สำหรับรัน
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# ตั้งค่า Environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000
ENTRYPOINT ["dotnet", "gameshop_api.dll"]