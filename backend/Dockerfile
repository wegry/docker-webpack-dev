
FROM microsoft/dotnet:2.0-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.fsproj ./
RUN dotnet restore

# copy and build everything else
COPY *.fs ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/backend.dll"]