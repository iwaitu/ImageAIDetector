#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN rm -f /etc/localtime \
&& ln -sv /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
&& echo "Asia/Shanghai" > /etc/timezone
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ImageAIDetector/ImageAIDetector.csproj", "ImageAIDetector/"]
COPY ["CustomVision/CustomVision.csproj", "CustomVision/"]
RUN dotnet restore "ImageAIDetector/ImageAIDetector.csproj"
COPY . .
WORKDIR "/src/ImageAIDetector"
RUN dotnet build "ImageAIDetector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageAIDetector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageAIDetector.dll"]