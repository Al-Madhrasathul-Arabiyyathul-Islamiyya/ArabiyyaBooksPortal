FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BooksPortal/ ./BooksPortal/
RUN dotnet restore ./BooksPortal/BooksPortal.slnx
RUN dotnet publish ./BooksPortal/src/BooksPortal.API/BooksPortal.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BooksPortal.API.dll"]
