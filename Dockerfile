FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY src/PaymentGateway.Web/*.csproj PaymentGateway.Web/
COPY src/PaymentGateway.Domain/*.csproj PaymentGateway.Domain/
COPY src/PaymentGateway.Persistence.InMemory/*.csproj PaymentGateway.Persistence.InMemory/
COPY src/PaymentGateway.Persistence.Api/*.csproj PaymentGateway.Persistence.Api/
COPY src/PaymentGateway.Acquirer.InMemory/*.csproj PaymentGateway.Acquirer.InMemory/
COPY src/PaymentGateway.Acquirer.Api/*.csproj PaymentGateway.Acquirer.Api/


RUN dotnet restore PaymentGateway.Web/PaymentGateway.Web.csproj -r linux-musl-x64

# Copy everything else and build
COPY src ./

WORKDIR /src/PaymentGateway.Web
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine
WORKDIR /app
COPY --from=build /app ./

ENV InMemoryAcquirer__AuthoriseBehaviour Approve

ENTRYPOINT ["./PaymentGateway.Web"]