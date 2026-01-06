FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JoyReactor.Accordion.WebAPI/JoyReactor.Accordion.WebAPI.csproj", "JoyReactor.Accordion.WebAPI/"]
COPY ["JoyReactor.Accordion.Logic/JoyReactor.Accordion.Logic.csproj", "JoyReactor.Accordion.Logic/"]
RUN dotnet restore "./JoyReactor.Accordion.WebAPI/JoyReactor.Accordion.WebAPI.csproj"
COPY . .
WORKDIR "/src/JoyReactor.Accordion.WebAPI"
RUN dotnet build "./JoyReactor.Accordion.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JoyReactor.Accordion.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JoyReactor.Accordion.WebAPI.dll"]