# Payment Gateway

This repo contains an example web application that plays the role of a payment gateway, mediating payment requests between merchants and an acquiring bank, whilst also offering reporting capabilities for the processed payments. 

## Running

The application is published as a docker image to the `ghcr.io/cmannix/payment-gateway` repository.

To run the latest version:

```bash
docker run -p 8080:80 ghcr.io/cmannix/payment-gateway:latest
```

This will start the image locally on port 8080. 

The service offers two endpoints:
  * `POST /payment` (Create a payment, returns a record of the payment, plus a result indicating whether the payment was approved or denied)

  * `GET /payment/{id}` (Retrieve a previously created payment by ID)

You can also access some HTTP request metrics at `/metrics`.


The API is (loosely) documented at `/swagger`, and the `scripts` directory contains example requests for both [Postman](https://www.postman.com) and `cURL`.

Note that the current implementation uses an in-memory data store for persisting the payments, so any created payments will not survive a service restart.

Similarly, the current implementation of the acquiring bank is a simple implementation that can be configured to always approve, deny, or error when approving a payment. The default behaviour is to always **Approve** a payment - this can be changed via config, either with appsettings.json, or environment variables like so:

```
docker run -p 8080:80 --env InMemoryAcquirer__AuthoriseBehaviour=Deny  ghcr.io/cmannix/payment-gateway:latest
```

Valid values for this config are `Approve`, `Deny`, `Error`.

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

## Implementation Notes

### Project Structure
Despite the relatively small scope of the features/public API offered by this service, I've attempted to structure the project in a manner that might be used in a much larger and more complex service. As such it might seem a little over-engineered - in reality for the first instance of a service this small I would keep the structure much simpler, hoping to develop more of an understanding of the domain before splitting things out like this.

### Acquirer abstraction
The abstraction for the acquirer has deliberately been kept as high-level as possible, and make no assumptions about what the API for the Acquirer might look like, given the large range of potential technologies that could be used there. This has the added benefit of keeping the existing code quite simple. Adding an implementation for a real acquirer would probably draw out some more information about use-cases/pitfalls, but without a real API to work against I think it's premature to add anything too specific.

### Payment persistence
As noted above, the payment 'persistence' is currently implemented as an in-memory store only, and thus won't survive a service restart. Clearly this would be impractical for production use! I belive it should be simple (i.e. not require any structural changes) to add implementations for more durable persistence options.

### Merchant

This initial implementation uses a hard coded set of merchant details for all requests. In reality, this gateway could be called by multiple merchants, and it would be important to be able to reliably identify the merchant associated with a request, both to be able to pass that information on to the acquiring/issuing bank (some merchants may be blocked), and to ensure data segregation (a merchant should only be able to retrieve their own payments). A token attempt has been made to show how this could be done (using the default merchant details) - in reality a variety of strategies could be used to identify the merchant, and segregate their data.

### Sensitive data
Under the assumption that in general we wish to minimise the amount of sensitive/PII data we store, the current payment persistence API receives a masked PAN and an expiry date only, rather than receiving and persisting the actionable card info.

The use of the `Sensitive` type is a bit hit and miss - I like the safety of not being able to accidentally log sensitive data, like the full card PAN, but it also added a fair amount of complexity, and in the case of things like the PAN, leads to some pretty ugly `Value.Value` calls in places. 

### Types and serialization
As much as possible, I've aimed to ensure that invalid data can't get into the system, e.g. by deserializing directly into 'tiny types' that constrain things like card PANs etc. This has lead to some complexity at the API layer (see the custome JsonConverters), but I think this is worthwhile in terms of having confidence that we aren't accidentally comparing the wrong strings and so on. A more developed 'tiny type' solution would reduce the complexity here a lot.

### Testing
I've added various automated tests, both at the unit level and at the 'integration' level using the MVC integration test helpers. If we anticipate a future evolution of the service to use a HTTP client for the acquirer, and a Postgres database for the payments repository, I would probably extend the integration tests (or add a new suite) to test against local versions of those dependencies, e.g. via TestContainers. Were this service being deployed I'd definitely want to add a proper end-to-end test suite that ran regularly against the deployed service.

### Logging/Metrics
The application publishes some basic metrics, including ASP.NET HTTP request metrics. It would be nice to add some more domain specific metrics too, for example how long to persist a payment, how long the acquirer takes etc.

The service uses the Microsoft logging abstractions, and uses log scopes to try and attach meaningful data to every request to aid debugging, including the payment ID, merchant name and merchant ID. The hope is that this should be useful for log filtering and searching in e.g. Kibana.

### Deploying/Versioning
Currently each push to main that passes the CI build will update the image tagged `latest` in the container registry. In reality it would be better to have a proper versioning strategy, probably managed via Git, either with files or tags. 

As it stands, the current output is a docker image, which is flexible but requires infrastructure configuration elsewhere. If we were deploying to e.g. Kubernetes, it might be more useful to produce a versioned helm chart that could also manage infrastructure around the service.

Similarly, again anticipating a more realistic future for this service involving a HTTP API for the acquirer, and a Postgres database for persistence, a `docker compose` file could be a useful addition for local dev/test.

