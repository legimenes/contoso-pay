# Contoso Pay

Payments gateway and backoffice for Contoso: the inbound payment flow (Gateway context) and the merchant-facing backoffice (Backoffice context).

## Language

### Identity and access

**Merchant**:
A store onboarded to Contoso Pay that sends payment traffic through the Gateway. It owns exactly one AppClient.
_Avoid_: store, loja, estabelecimento, customer, tenant, account

**AppClient**:
The client-id and client-secret pair a Merchant uses to authenticate against the Gateway. A Merchant has exactly one; rotating a secret updates the existing AppClient instead of creating another.
_Avoid_: OAuth client, application, API key, credential, merchant key
