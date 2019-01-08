FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /IngestionImporter

COPY ./WebApi/WebApi.csproj ./WebApi/

RUN dotnet restore ./WebApi/WebApi.csproj

# RUN dotnet add ./WebApi/WebApi.csproj package -s http://localhost:8001 Domain

COPY ./WebApi.Tests ./WebApi.Tests/

RUN dotnet restore ./WebApi.Tests/WebApi.Tests.csproj

# RUN dotnet add ./WebApi.Tests/WebApi.Tests.csproj package -s http://localhost:8001 Domain

COPY . .

RUN dotnet test

RUN dotnet publish  ./WebApi/WebApi.csproj -c release -o ../../publish

FROM microsoft/dotnet:2.1-aspnetcore-runtime

WORKDIR /publish

COPY --from=build ./publish .

ENTRYPOINT [ "dotnet", "WebApi.dll" ]
