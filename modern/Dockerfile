FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY HotelManager.sln ./
COPY src/HotelManager.Domain/HotelManager.Domain.csproj src/HotelManager.Domain/
COPY src/HotelManager.Application/HotelManager.Application.csproj src/HotelManager.Application/
COPY src/HotelManager.Infrastructure/HotelManager.Infrastructure.csproj src/HotelManager.Infrastructure/
COPY src/HotelManager.Web/HotelManager.Web.csproj src/HotelManager.Web/
RUN dotnet restore src/HotelManager.Web/HotelManager.Web.csproj
COPY src/ src/
RUN dotnet publish src/HotelManager.Web/HotelManager.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080 \
    HotelManager__DbPath=/data/hotelmanager.db
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "HotelManager.Web.dll"]
