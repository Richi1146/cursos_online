FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OnlineCourses.Api/OnlineCourses.Api.csproj", "OnlineCourses.Api/"]
COPY ["OnlineCourses.Application/OnlineCourses.Application.csproj", "OnlineCourses.Application/"]
COPY ["OnlineCourses.Infrastructure/OnlineCourses.Infrastructure.csproj", "OnlineCourses.Infrastructure/"]
COPY ["OnlineCourses.Domain/OnlineCourses.Domain.csproj", "OnlineCourses.Domain/"]
RUN dotnet restore "OnlineCourses.Api/OnlineCourses.Api.csproj"
COPY . .
WORKDIR "/src/OnlineCourses.Api"
RUN dotnet build "OnlineCourses.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineCourses.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineCourses.Api.dll"]
