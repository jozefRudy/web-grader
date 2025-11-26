FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build-image

WORKDIR /src

# Copy solution and projects with correct structure
COPY WebGrader.slnx .
COPY WebGrader.Core/ WebGrader.Core/
COPY WebGrader.Api/ WebGrader.Api/
COPY WebGrader.Tests/ WebGrader.Tests/

# Restore dependencies
RUN dotnet restore WebGrader.slnx

# Build
RUN dotnet build WebGrader.slnx -c Release

# Publish only the API project
RUN dotnet publish WebGrader.Api/WebGrader.Api.fsproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build-image /app/out .

ENTRYPOINT ["dotnet", "WebGrader.Api.dll"]
