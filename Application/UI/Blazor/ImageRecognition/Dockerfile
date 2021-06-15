FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Clients/ImageRecognition.BlazorFrontend/ImageRecognition.BlazorFrontend.csproj", "Clients/ImageRecognition.BlazorFrontend/"]
COPY ["Clients/ImageRecognition.API.Client/ImageRecognition.API.Client.csproj", "Clients/ImageRecognition.API.Client/"]
COPY ["Communication/ImageRecognition.Communication.Manager/ImageRecognition.Communication.Manager.csproj", "Communication/ImageRecognition.Communication.Manager/"]

RUN dotnet restore "Clients/ImageRecognition.BlazorFrontend/ImageRecognition.BlazorFrontend.csproj"
COPY . .
WORKDIR "/src/Clients/ImageRecognition.BlazorFrontend"
RUN dotnet build "ImageRecognition.BlazorFrontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageRecognition.BlazorFrontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageRecognition.BlazorFrontend.dll"]