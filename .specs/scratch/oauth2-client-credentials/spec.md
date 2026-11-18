# OAuth 2.0 client credentials for the Gateway API

Status: ready-for-agent

## Problem Statement

A Merchant has no way to authenticate against the Gateway. The API currently exposes only anonymous health checks and documentation, and authentication and authorization are commented out in the composition root. Every future business endpoint therefore has to either stay open or invent its own credential scheme.

The Gateway needs one standard, auditable way for a Merchant's AppClient to exchange its credentials for a short-lived access token, plus a single enforcement point that every business endpoint can rely on without reimplementing token validation.

## Solution

Expose a standards-compliant OAuth 2.0 token endpoint — `POST /connect/token`, unversioned, `client_credentials` grant only — served by the Gateway API, with the authorization server implemented by OpenIddict inside the `Adapters.Authorization` adapter.

A Merchant sends its AppClient `client_id` and `client_secret` in the request body and receives a JWT access token valid for 10 minutes by default, whose audience is `gateway-api`. The Gateway validates that token in-process and requires it on every business endpoint through a single authorization policy. The token endpoint, health checks and documentation remain anonymous.

AppClients live in the existing `contoso` database, in the `contosopay` schema, provisioned by a numbered SQL script. Tokens are persisted, signing uses a development certificate, and a Development-only seed creates the first AppClient so the flow can be exercised locally.

## User Stories

1. As a Merchant, I want to exchange my AppClient credentials for an access token, so that I can call Gateway endpoints.
2. As a Merchant, I want to send `client_id` and `client_secret` in the request body, so that an ordinary form-encoded HTTP client works without special header encoding.
3. As a Merchant, I want the token endpoint at a stable unversioned path, so that my integration does not break when the Gateway versions its business APIs.
4. As a Merchant, I want a pure RFC 6749 token response, so that off-the-shelf OAuth 2.0 client libraries work unchanged.
5. As a Merchant, I want to learn the token lifetime from `expires_in`, so that I can obtain a new token before the current one expires.
6. As a Merchant, I want invalid credentials rejected with HTTP 401 and `invalid_client`, so that I can distinguish an authentication failure from a malformed request or a server error.
7. As a Merchant, I want my access token to identify my AppClient through `sub`, so that Gateway logs and endpoints can attribute traffic to the correct credential.
8. As a Merchant, I want my token bound to the Gateway API through `aud=gateway-api`, so that it cannot be replayed against a different Contoso Pay API.
9. As a Merchant, I want exactly one AppClient per Merchant, so that there is no ambiguity about which credential represents me.
10. As a Merchant, I want a credential change to invalidate the previous secret immediately, so that a leaked secret stops working as soon as it is replaced.
11. As a Merchant, I want business endpoints to reject requests without a valid token, so that my data is not exposed by omission.
12. As a Merchant, I want no refresh token and no fields beyond the RFC 6749 success response, so that the contract stays small and predictable.
13. As a Merchant, I want the token to carry no merchant data yet, so that the credential is not coupled to a Merchant model that does not exist.
14. As a Merchant, I want to obtain a token with a single request without negotiating scopes, so that the integration does not depend on a permission catalogue.
15. As a Gateway developer, I want `Adapters.Authorization` to be a class library that registers OpenIddict through an extension method, so that the composition root stays a thin shim.
16. As a Gateway developer, I want the exchange logic behind the `IExchangeToken` port, so that the API layer does not depend on OpenIddict.
17. As a Gateway developer, I want that port to accept only a client identifier, so that framework request types never leak into the Application project.
18. As a Gateway developer, I want server and validation to run in the same process, so that there is no second host, no discovery call and no shared encryption key to manage.
19. As a Gateway developer, I want issued tokens persisted, so that revocation and audit become possible without a schema rewrite.
20. As a Gateway developer, I want table names without the framework prefix and in lowercase, so that the schema matches the project's naming style and needs no quoted identifiers.
21. As a Gateway developer, I want the mappings in `IEntityTypeConfiguration` classes, so that the table and column decisions are explicit and discoverable.
22. As a Gateway developer, I want the authorization tables isolated in the `contosopay` schema, so that framework-owned objects stay separate from application objects.
23. As a Gateway developer, I want a dedicated numbered SQL script executed manually, so that the database state is reviewable in git and reproducible in a fresh environment.
24. As a Gateway developer, I want no EF Core migrations, so that the repository's documented schema workflow is preserved.
25. As a Gateway developer, I want an idempotent Development seed, so that a fresh local environment can call the token endpoint without manual SQL.
26. As a Gateway developer, I want the seed to skip AppClients that already exist, so that restarts do not duplicate clients or overwrite a rotated secret.
27. As a Gateway developer, I want the seed restricted to Development, so that production never starts with a known client secret.
28. As a Gateway developer, I want token lifetime and issuer to come from configuration, so that environments can be tuned without code changes.
29. As a Gateway developer, I want a development signing certificate, so that I can run the flow locally without provisioning keys.
30. As a Gateway developer, I want the audience registered on the server and validated strictly, so that a token minted for another audience is rejected.
31. As a security reviewer, I want client secrets stored hashed, so that a database leak does not expose usable credentials.
32. As a security reviewer, I want the token endpoint, health checks and documentation to be the only anonymous surfaces, so that everything else requires a valid token.
33. As a security reviewer, I want the token endpoint's errors to remain RFC 6749 shaped, so that credential probing cannot be confused with the API's error envelope.
34. As a QA engineer, I want an integration test that runs against a real PostgreSQL instance, so that the SQL script and the EF Core model cannot drift silently.
35. As a QA engineer, I want the happy path verified through real HTTP, so that routing, form binding, exchange, signing and response serialization are proven together.
36. As a QA engineer, I want the invalid-secret path verified, so that the error contract is proven and not assumed.
37. As a QA engineer, I want a controllable clock, so that the configured token lifetime is verified deterministically and without waiting.
38. As an operator, I want health checks and documentation to stay public, so that monitoring and onboarding do not need credentials.
39. As a new developer, I want the persistence documentation to describe numbered scripts per concern, so that the written rule matches the repository.
40. As an API consumer, I want the OpenAPI document to show the Bearer scheme and point at the token endpoint, so that I can try the flow from the documentation.

## Implementation Decisions

### Components and dependency direction

- `Adapters.Authorization` is an outbound adapter implemented as a class library. It owns OpenIddict, the authorization `DbContext`, the entity configurations and the exchange implementation. It exposes a DI extension that registers the server, the validation, the EF Core stores, authentication and authorization.
- The Gateway API composition root calls that extension and enables the authentication and authorization middleware. No OpenIddict type appears in the API layer, except through the registration extension.
- `Gateway.Application` owns the driven port `IExchangeToken` in its cross-cutting abstractions area, with the shape `Task<Result<ClaimsPrincipal>> ExchangeAsync(string clientId, CancellationToken cancellationToken)`. It receives a client identifier that OpenIddict has already authenticated and returns the principal to attach to the token.
- `Adapters.Authorization` implements the port on top of `IOpenIddictApplicationManager`, adding `sub` (client identifier), `name` (display name) and the audience to the principal.
- `Gateway.API` owns the HTTP endpoint `POST /connect/token` as an unversioned endpoint tagged `Connect`, marked anonymous. It binds the form, calls the port, signs in with the OpenIddict server scheme, and contains no protocol logic.

### Token endpoint contract

- Path is `POST /connect/token`, with no version prefix. The request is `application/x-www-form-urlencoded` with `grant_type=client_credentials`, `client_id` and `client_secret`.
- The documented contract authenticates with credentials in the body; it does not rely on an `Authorization: Basic` header.
- Success returns HTTP 200 with `access_token`, `token_type=Bearer` and `expires_in` equal to the configured lifetime in seconds (600 by default). No `refresh_token`, no `scope`, no custom fields.
- Invalid or unknown credentials return HTTP 401 with an RFC 6749 error body whose `error` is `invalid_client`. These responses are produced by the server and are the one deliberate exception to the API's `ProblemDetails` envelope.
- `UseStatusCodePages()` must not decorate the token endpoint responses; the 4xx bodies from the server are already complete.
- Access-token claims: `iss` from configuration, `sub` equal to the AppClient client identifier, `name` equal to the AppClient display name, `aud` equal to `gateway-api`, plus the standard `iat` and `exp`.
- No `merchant_id`, no `scope`/`scp` and no custom claims in this feature.

### Authorization and audience

- The only audience is `gateway-api`. It is registered on the server, attached to the principal during the exchange and required during validation.
- Validation is strict: a token with a different audience, or with no audience, is rejected. The standard JWT audience check is left to OpenIddict, which performs it itself.
- Tokens are validated in-process against the same database the server writes to, using the local server validation mode. There is no discovery document, no separate authorization host and no shared key handshake.
- A single named authorization policy represents "valid Gateway token" and is applied to business endpoints. Health checks, documentation and the token endpoint are explicitly anonymous.
- On the server side, audience validation remains enabled; on the validation side, the expected audience is declared explicitly. The audience value is a stable identifier, not a host name or port.

### Lifetime and signing

- Access-token lifetime is 10 minutes by default and configurable.
- The issuer is configurable and stable across environments; it is not derived from the request host.
- Signing uses the development signing certificate. A production certificate and key-management story are explicitly pending, not silently assumed.

### Persistence

- Database: the existing `contoso` database. Schema: `contosopay`.
- Four tables: `applications`, `authorizations`, `scopes`, `tokens` — lowercase, unquoted, without the `OpenIddict` prefix.
- `applications` stores the AppClient: client identifier, secret hash and display name. `tokens` stores issued tokens. `authorizations` and `scopes` exist but stay empty in this feature.
- Mappings are declared in `IEntityTypeConfiguration` classes inside the adapter. The OpenIddict model is registered first, and the explicit configurations are applied so that the table names above win over the framework defaults.
- The DDL lives in a dedicated numbered script for the OpenIddict objects, distinct from the application script. It is executed manually; the Docker init hook never runs it.
- No EF Core migrations.
- The persistence documentation changes from "a single `01-objects.sql`" to "numbered scripts per concern", so that the documented convention matches the repository.

### AppClient provisioning

- A Development-only, idempotent seed creates the initial AppClient from a configuration section containing client identifier, client secret and display name. It uses `IOpenIddictApplicationManager`, so the secret is hashed through the same path as any future credential write.
- The seed skips any client identifier that already exists and never overwrites an existing secret.
- No endpoint returns the secret after creation. The plaintext secret exists only in Development configuration and documentation; losing it means rotating it, which belongs to the Backoffice.
- The test environment does not use the seed; it creates its own AppClient through the same manager.

### Configuration surface

- An authorization configuration section carries issuer, access-token lifetime and Development seed entries.
- The existing PostgreSQL connection string is reused; no second connection string is introduced for the authorization store.
- In the `Testing` environment, logging must be Console-only. The current logging registration adds the MongoDB sink unconditionally, so the registration becomes environment-aware for this decision to hold. The MongoDB connection string remains required by the existing configuration validation and is supplied as a placeholder by tests.

### Documentation

- The persistence documentation is updated for numbered scripts per concern.
- The OpenAPI security scheme continues to describe the Bearer token as coming from `POST /connect/token`.

## Testing Decisions

- A good test here observes external behaviour only: HTTP status, response body and headers, and the decoded token claims. It must not assert OpenIddict internals, read the authorization tables directly, or depend on implementation types. The one non-HTTP interaction allowed is creating the AppClient through `IOpenIddictApplicationManager` as test arrangement.
- There is a single seam: the running Gateway API behind `WebApplicationFactory<Program>`, backed by a real PostgreSQL Testcontainer, in `Gateway.Tests`. No unit tests are written for the exchange adapter; it is a thin wrapper and the HTTP seam already exercises it end to end.
- The fixture starts the container, executes the OpenIddict SQL script against it, points the application's PostgreSQL connection string at the container, sets the environment to `Testing`, and replaces the clock with the existing fake `TimeProvider`.
- The SQL script is executed from the script itself rather than generated from the model, on purpose: a mapping change without a matching DDL delta must break the test.
- Integration cases:
  1. **Valid AppClient**: HTTP 200, non-empty `access_token`, `token_type` of `Bearer`, `expires_in` of 600, and a decoded JWT whose `iss` matches the configured issuer, whose `sub` is the client identifier and whose `aud` is `gateway-api`.
  2. **Invalid secret**: HTTP 401 with `error=invalid_client` and no access token.
  3. **Configured lifetime**: with the fake clock pinned, the decoded token's `iat` equals the test instant and `exp - iat` equals 600 seconds, consistent with `expires_in`.
- Accepted limitation: enforcement of an expired token (HTTP 401 from a protected endpoint) is not covered, because no protected business endpoint exists yet. The first business endpoint must add that case.
- Test names follow `MethodName_When[Condition]_Should[ExpectedBehavior]`, bodies use Given/When/Then comments, and tests carry an integration trait under the authorization capability.
- Prior art: the existing `TestingWebApplicationFactory`, `TestTimeProvider` and the repo's Given/When/Then plus trait conventions.

## Out of Scope

- The Backoffice context: Merchant onboarding, status, AppClient issuance and rotation, and the `merchant_id` claim.
- Authorization code and refresh token flows, PKCE, consent, introspection, revocation and discovery metadata.
- Scopes, permission catalogues and per-scope policies.
- Protected business endpoints, and with them the 401-for-expired-token case.
- Rate limiting, brute-force protection and an audit trail of token issuance.
- The production signing certificate and key management.
- MongoDB logging verification and a containerized MongoDB for tests.
- Production deployment, secrets management and key rotation procedures.

## Further Notes

- The token endpoint is the only place in the API where the `ProblemDetails` envelope does not apply; RFC 6749 owns that error shape. This should be stated where the error-handling conventions live so nobody "fixes" it later.
- OpenIddict stores only the secret hash. A lost secret cannot be recovered, only rotated.
- Three decisions are already recorded as accepted ADRs: the provider inside the Gateway API, one AppClient per Merchant, and the single database with numbered SQL scripts.
- Existing technical debt touched by this feature: the entry point must be public to be usable by `WebApplicationFactory`, the Testing settings must describe the test environment, and logging must not require a MongoDB instance in tests.
- The concrete DDL delta, class and field inventory, package versions and file layout belong to the technical plan that precedes implementation, per the repository's implement gate.
