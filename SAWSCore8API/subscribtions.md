# Subscriptions (SAWSCore8API)

This document describes how subscriptions are managed in the `SAWSCore8API` project, including PayFast payment initiation, PayFast ITN (Instant Transaction Notification) processing, and the configuration values required.

## Key components

### API surface
Subscription and PayFast-related endpoints are implemented in `Controllers/SubscriptionsController.cs`.

Base route:
- `api/v1/Subscriptions`

PayFast return/cancel landing pages are implemented in:
- `Controllers/PayFastRedirectController.cs`

Return/cancel base route:
- `v1/subscriptions/payfast`

### Business logic
Most subscription/payment logic lives in `Services/SubscriptionService.cs`, exposed via `Interfaces/ISubscriptionService.cs`.

### Persistence
Subscriptions are stored in the SQL database using EF Core via `DbContexts/SAWSDbContext`.

The main persisted entity is `Models/Subscription.cs` (table: `Subscription`).

## Data model

### `Subscription`
`Models/Subscription.cs` represents the subscription record.

Important fields used by the current process:
- `subscriptionId`: primary key
- `userprofileid`: user profile FK/identifier
- `package_name`, `package_id`, `package_price`
- `start_date`, `end_date`, `subscription_duration`
- `subscription_token`: PayFast subscription token (used for cancellation)
- `isactive`: indicates currently active subscription
- soft delete: `isdeleted`, `deleted_at`

### `Payment`
`Models/Payment.cs` is used to initiate PayFast payments (recurring/once-off/ad-hoc).

Notable fields:
- Buyer: `name_first`, `name_last`, `email_address`
- Transaction: `m_payment_id`, `amount`, `item_name`, `item_description`
- Recurring: `recurring_amount`, `frequency` (expected: `monthly` or other => annual)
- Subscription metadata (sent to PayFast as custom fields):
  - `userId`
  - `package_id`
  - `subscription_amount`
  - `package_name`
  - `subscription_type`
- Notify callback URL: `notifyUrl` (server-to-server ITN)

Important change:
- Client-provided `returnUrl` and `cancelUrl` are no longer trusted/required. The backend overwrites them with HTTPS URLs generated from configuration.

## Dependency injection / registration

Services are registered in `Configurations/DependencyInjectionConfig.cs`.

Notable registrations:
- `services.AddScoped<ISubscriptionService, SubscriptionService>();`
- `services.AddHttpClient();`

PayFast options binding (new):
- `services.Configure<PayFastOptions>(configuration.GetSection("PayFast"));`

This is why `Program.cs` calls:
- `builder.Services.ResolveDependencies(builder.Configuration);`

The PayFast options-class is:
- `Options/PayFastOptions.cs`

## Subscription management endpoints (non-payment)

### Create or update subscription
Endpoint:
- `POST api/v1/Subscriptions/PostInsertSubscription`

Behavior:
- If `subscription.subscriptionId == 0` => creates a new subscription via `ISubscriptionService.CreateSubscription`.
- Else => updates an existing subscription via `ISubscriptionService.UpdateSubscription`.

Validation:
- Uses `ModelState` validation.
- Returns a `CreateResult` containing per-field validation errors if invalid.

Persistence behavior (`SubscriptionService`):
- `CreateSubscription` sets:
  - `created_at = DateTime.Now`
  - `updated_at = DateTime.Now`
  - `isdeleted = false`
- `UpdateSubscription` sets:
  - `updated_at = DateTime.Now`
  - `isdeleted = false`

### Get active subscription by user
Endpoint:
- `GET api/v1/Subscriptions/GetActiveSubscriptionByUserProfileId?id={userProfileId}`

Behavior:
- Returns the first subscription where `userprofileid == id` and `isactive == true`.

### Get subscription by id
Endpoint:
- `GET api/v1/Subscriptions/GetSubscriptionById?id={subscriptionId}`

### Soft delete subscription by id
Endpoint:
- `DELETE api/v1/Subscriptions/DeleteSubscriptionById?id={subscriptionId}`

Behavior (`SubscriptionService.DeleteSubscriptionById`):
- Marks as deleted:
  - `isdeleted = true`
  - `deleted_at = DateTime.Now`

## PayFast payment initiation endpoints

All payment initiation endpoints accept a JSON `Payment` body.

### Important: return/cancel URLs are generated on the backend
PayFast rejects non-HTTPS return/cancel URLs.

To avoid issues with mobile deep links (e.g. `capacitor://localhost`) and HTTP-only admin portals, the backend always sets:
- `return_url = {PayFast:PublicBaseUrl}/v1/subscriptions/payfast/return`
- `cancel_url = {PayFast:PublicBaseUrl}/v1/subscriptions/payfast/cancel`

Client values for `returnUrl`/`cancelUrl` are ignored/overwritten.

A guard is implemented: if `PayFast:PublicBaseUrl` is missing or not HTTPS, the API logs an error and fails payment initiation with a clear message.

### Recurring payment
Endpoint:
- `POST api/v1/Subscriptions/RecurringPayment`

Service method:
- `SubscriptionService.RecuringPayment`

What it does:
1. Validates `PayFast:PublicBaseUrl` is present and `https://`.
2. Builds a `PayFastRequest` (from `PayFast.AspNetCore`) using configured passphrase.
3. Sets merchant fields.
4. Sets **backend-generated** HTTPS `return_url` and `cancel_url`.
5. Keeps `notify_url` as the server-to-server callback:
   - `notify_url = request.notifyUrl`
6. Sends subscription metadata using PayFast custom fields:
   - `custom_int1 = request.userId`
   - `custom_int2 = request.package_id`
   - `custom_int3 = request.subscription_amount`
   - `custom_str1 = request.package_name`
   - `custom_str2 = request.subscription_type`
7. Sets recurring billing fields (`SubscriptionType.Subscription`, frequency monthly/annual).
8. Returns a redirect URL (string) to the caller.

### Once-off payment
Endpoint:
- `POST api/v1/Subscriptions/OnceOffPayment`

Service method:
- `SubscriptionService.OnceOffPayment`

What it does:
- Same backend URL generation for `return_url` and `cancel_url`.
- `notify_url = request.notifyUrl`.

### Ad-hoc payment
Endpoint:
- `POST api/v1/Subscriptions/AdHocPayment`

Service method:
- `SubscriptionService.AdHocPayment`

What it does:
- Same backend URL generation for `return_url` and `cancel_url`.
- `notify_url = request.notifyUrl`.
- Sets `subscription_type = SubscriptionType.AdHoc`.

## PayFast return/cancel endpoints (new)

PayFast will redirect the user’s browser to these endpoints after payment/cancellation.

### Return endpoint
- `GET /v1/subscriptions/payfast/return`

### Cancel endpoint
- `GET /v1/subscriptions/payfast/cancel`

Behavior:
- Returns a minimal HTML page containing:
  - Message: “You can return to the app”
  - Script that attempts `window.close()` (useful for in-app browsers)
  - Fallback text for cases where closing is blocked

Query parameters:
- These endpoints allow any PayFast query parameters (e.g. `pf_payment_id`, `payment_status`, etc.). No strict model binding occurs.

## PayFast ITN (Instant Transaction Notification)

### ITN handler endpoint
Endpoint:
- `POST api/v1/Subscriptions/Notify`

Binding:
- Uses `[ModelBinder(BinderType = typeof(PayFastNotifyModelBinder))]` to bind form-posted ITN data into a `PayFastNotify` object.

Flow:
1. Controller receives the ITN payload as `PayFastNotify`.
2. Controller calls `SubscriptionService.NotifyITN(payFastNotify)`.
3. Service dispatches by `payFastNotify.payment_status`:
   - `COMPLETE` => `HandleSuccessfulPayment(payFastNotify)`
   - `FAILED` => `HandleFailedPayment()`
   - `PENDING` => `HandlePendingPayment()`

### Successful payment handling (`HandleSuccessfulPayment`)
On `payment_status == "COMPLETE"`:

1. Reads subscription metadata from PayFast custom fields:
   - `custom_int1` => `userId`
   - `custom_int2` => `packageId`
   - `custom_int3` => `packagePrice`
   - `custom_str1` => `package_name`
   - `token` => stored as `subscription_token`

2. Creates a new `Subscription` record:
   - `start_date = DateTime.Now`
   - `end_date = DateTime.Now.AddMonths(12)`
   - `subscription_duration = 365`
   - `isactive = true`

3. Attempts to locate an existing active subscription for the user.

4. If no existing active subscription exists:
   - Creates the new subscription and returns success.

5. If an active subscription exists:
   - Attempts to cancel it on PayFast via `CancelSubscription(activeSubscription.subscription_token)`.
   - Creates the new subscription.
   - Marks previous subscription `isactive = false` and updates it.

## Cancel subscription flow

Endpoint:
- `POST api/v1/Subscriptions/CancelSubscription?subscriptionId={id}`

Flow:
1. Looks up subscription by id.
2. Calls `ISubscriptionService.CancelSubscription(subscription.subscription_token)` to cancel on PayFast.
3. Marks subscription inactive in DB (`isactive = false`) and updates.
4. Creates a free subscription for the same user via `CreateFreeSubscription(userId)`.

## Configuration

### PayFast configuration values
PayFast settings are stored under the `payFast` section in:
- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`

New key (required for payment initiation):
- `payFast:PublicBaseUrl`
  - must be an absolute `https://` URL
  - must be publicly reachable by PayFast
  - used to construct:
    - `/v1/subscriptions/payfast/return`
    - `/v1/subscriptions/payfast/cancel`

Other keys used by code:
- `payFast:merchant_id`
- `payFast:merchant_key`
- `payFast:passphrase`
- `payFast:endPoint`
- `payFast:isTesting`

Notes:
- `payFast:ReturnUrl` and `payFast:CancelUrl` still exist in config in some environments, but are no longer used for initiating payments.
- `notify_url` is still taken from `Payment.notifyUrl` (server-to-server ITN endpoint).

## End-to-end summary (typical paid subscription upgrade)

1. Client initiates payment:
   - Calls `POST api/v1/Subscriptions/RecurringPayment` (or once-off/ad-hoc), receives redirect URL.
2. Client is redirected to PayFast to complete payment.
3. PayFast redirects the browser to:
   - `GET /v1/subscriptions/payfast/return` or `GET /v1/subscriptions/payfast/cancel`
   - The HTML page attempts to close the in-app browser window.
4. PayFast posts ITN to `api/v1/Subscriptions/Notify` (server-to-server).
5. API creates a new paid `Subscription` record and deactivates the previous active subscription.
6. Optional: user/admin cancels subscription via `CancelSubscription`, which cancels PayFast token and provisions a free fallback subscription.
