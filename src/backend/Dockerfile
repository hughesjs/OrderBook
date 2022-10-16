# Non-standard build and publish as the .net 7 publish is still a bit glitchy

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ./backend/OrderBookService ./backend/OrderBookService
COPY ./shared ./shared

WORKDIR ./backend/OrderBookService
RUN dotnet build -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT "./OrderBookService"
