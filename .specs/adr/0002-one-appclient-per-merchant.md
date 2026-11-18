---
Status: accepted
---

# A Merchant has exactly one AppClient

A Merchant authenticates against the Gateway with exactly one `AppClient`, the pair of client-id and client-secret. Rotating the secret updates the existing `AppClient` in place; it does not issue a second one. The Merchant lifecycle itself — onboarding, status, credential issuance — belongs to the Backoffice context and is outside the current scope.

## Considered Options

- **One AppClient per application or environment** (a merchant with a sandbox pair and a production pair, or one pair per integrated system). Rejected for now: it multiplies credential lifecycle states before there is a Backoffice to manage them.
- **Credentials attached only to a technical OAuth client, with Merchant as a separate concept referenced by claim.** Rejected: it models two things where the domain has one identity to authenticate.

## Consequences

- A merchant cannot hold two valid credential pairs at the same time, so rotation is not overlapped; an old secret stops working the moment the new one is written.
- Reintroducing multiple credentials per merchant later is a model change, not a configuration change.
