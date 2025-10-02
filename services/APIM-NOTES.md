# API Management (APIM) Notes

- Import each microservice OpenAPI (`openapi.users.json`, `openapi.games.json`, `openapi.payments.json`) as separate APIs.
- Configure policies:
  - Validate JWT: set issuer/audience to Users service values.
  - CORS: allow `http://localhost:4200` and `https://*.azurestaticapps.net`.
  - Rate limit and retry as needed.
- Suggested routes:
  - Users: `/users/*`
  - Games: `/games/*`
  - Payments: `/payments/*`
- Optionally create a single gateway API routing to backends with path-based rules.
