---
Status: accepted
---

# OAuth 2.0 provider lives inside the Gateway API

Contoso Pay needs an OAuth 2.0 authorization server for the `client_credentials` flow. `Adapters.Authorization` is an outbound adapter (a class library), not a host, and OpenIddict requires ASP.NET Core hosting plus a custom exchange handler: it authenticates `client_id`/`client_secret` by itself but does not issue a token until a handler attaches a principal. We therefore register the OpenIddict server from `Adapters.Authorization` through a DI extension, expose `POST /connect/token` as an unversioned endpoint in `Gateway.API`, implement the exchange behind the `IExchangeToken` port in the adapter, and validate access tokens in the same process with `UseLocalServer()`.

## Considered Options

- **A dedicated `Authorization.API` host**, with `Gateway.API` validating tokens through OpenID Connect discovery plus a shared encryption key. Rejected: two processes, two lifetimes, a key handshake and a network hop for a flow that has exactly one consumer.
- **`Gateway.API` calling `IOpenIddictApplicationManager` directly** from the endpoint. Rejected: it puts a framework dependency and protocol logic in the entry point, against the rule that the API only routes and maps HTTP to contracts.
- **A hand-rolled token endpoint** calling a service that mints JWTs. Rejected: it reproduces RFC 6749 error handling, client authentication and token lifecycle without the library that already does it correctly.

## Consequences

- Authorization server and resource server share one process and one deployment. Scaling them independently, or moving the provider to its own host, means revisiting this decision.
- Token validation needs no issuer discovery, no shared encryption key and no revocation round trip, because validation reads the same database the server writes.
- Signing keys are per-machine development certificates until production credentials are specified; every token becomes invalid when that key changes.
