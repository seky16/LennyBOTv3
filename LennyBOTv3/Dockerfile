#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LennyBOTv3/LennyBOTv3.csproj", "LennyBOTv3/"]
RUN dotnet restore "LennyBOTv3/LennyBOTv3.csproj"
COPY . .
WORKDIR "/src/LennyBOTv3"
RUN dotnet build "LennyBOTv3.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LennyBOTv3.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LennyBOTv3.dll"]