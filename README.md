# Payment Gateway

This repo contains an example web application that plays the role of a payment gateway, mediating payment requests between merchants and an acquiring bank, whilst also offering reporting capabilities for the processed payments. 

## Running

The application is published as a docker image to the `ghcr.io/cmannix/payment-gateway` repository.

To run the latest version:

```bash
docker run -p 8080:80 ghcr.io/cmannix/payment-gateway:latest
```

This will start the image locally on port 8080. The API is (loosely) documented at `/swagger`, and there is a Postman collection in the `scripts` repository that contains some example requests.

## Local development

### Requirements
You'll need at least v5.0.201 of the dotnet SDK (https://dotnet.microsoft.com/download/dotnet/5.0, see `global.json` in the root)

To build the entire solution, from repo root run:

```
dotnet build
```

To run all the tests, from repo root run:

```
dotnet test
```

You can build and run the application directly by running (from repo root)

```bash
dotnet run --project src/PaymentGateway.Web/PaymentGateway.Web.csproj
```

Alternatively, you can build/run the docker image locally with

```bash
docker build -t payment-gateway .
docker run -p 8080:80 payment-gateway
```

## Deploying

The CI build is handled by Github Actions, and on every push to `main` that passes the tests will update the image tagged as `latest`. There's no versioning strategy beyond that. Untagged images should be cleaned up manually.