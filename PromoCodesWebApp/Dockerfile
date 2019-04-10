FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
#EXPOSE 44327

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["PromoCodesWebApp.csproj", "./"]
RUN dotnet restore "./PromoCodesWebApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PromoCodesWebApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PromoCodesWebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PromoCodesWebApp.dll"]