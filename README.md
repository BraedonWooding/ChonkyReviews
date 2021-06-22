# Chonky Reviews

## Open Source Implementation of Google My Business API

![Build Workflow](https://github.com/BraedonWooding/ChonkyReviews/actions/workflows/dotnet.yml/badge.svg)

[Documentation (Swagger)](https://braedonwooding.github.io/ChonkyReviews/)

> Currently we only support a basic subset of the API, with attention only to reviews.

The swagger link that was above, details the API.  There is the `/api` base path and the `/mock/` base path;

`/mock` offers no guarantees on it being similar to google's API, where as `/api` attempts to mock it as well as possible within the scope of what has been implemented.

## Implemented Endpoints;

No authorization scope logic is implemented, furthermore no *real* permission/security driven calls are implemented.  The actual endpoints are shown in the swagger.

### My Business API v4

- [x] [`v4/accounts.locations.reviews`](https://developers.google.com/my-business/reference/rest/v4/accounts.locations.reviews) and all subpath api calls
- [x] [`v4/accounts.locations.list`](https://developers.google.com/my-business/reference/rest/v4/accounts.locations/list) list locations.
- [x] [`v4/accounts.list`]

### My Business API Account Management v1

- [x] [`accountmanagement/v4/accounts.list`](https://developers.google.com/my-business/reference/accountmanagement/rest/v1/accounts/list) list all accounts
  - This is the new recommended way to get accounts (old deprecated way is `v4/accounts.list`)
