# Etap 1: Budowanie aplikacji (używamy obrazu SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiujemy plik solucji i pliki projektu, aby przywrócić zależności (restore)
# Dzięki temu Docker cache'uje warstwę z paczkami NuGet
COPY ["Market.sln", "./"]
COPY ["Market.Web/Market.Web.csproj", "Market.Web/"]

# Pobieranie zależności
RUN dotnet restore "Market.sln"

# Kopiujemy resztę kodu źródłowego
COPY . .

# Budujemy aplikację w trybie Release
WORKDIR "/src/Market.Web"
RUN dotnet build "Market.Web.csproj" -c Release -o /app/build

# Publikujemy aplikację (tworzymy pliki wynikowe gotowe do uruchomienia)
FROM build AS publish
RUN dotnet publish "Market.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etap 2: Uruchomienie (używamy lekkiego obrazu Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Zmieniamy port na 5000
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Kopiujemy opublikowane pliki z etapu budowania
COPY --from=publish /app/publish .

# Tworzymy katalog na zdjęcia (jeśli nie istnieje) i nadajemy uprawnienia
# Ważne: W Coolify musisz zamontować ten katalog jako wolumen, aby zdjęcia nie znikały!
RUN mkdir -p /app/wwwroot/uploads && chmod 777 /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "Market.Web.dll"]