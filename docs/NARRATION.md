# Study guide: explaining this port

This is **not a script to read verbatim** — it's the set of points you should be able to
explain in your own words if asked. If you can walk through each section below without
looking, you understand the code well enough to record the video yourself.

## 1. What this is

- Original: [Roberto-Celano/MVCbase](https://github.com/Roberto-Celano/MVCbase) — a small
  (~21 file) PHP MVC *teaching* skeleton. Its own README says "basic structure ready,"
  not a finished app.
- This port: ASP.NET Core MVC on .NET 9, same feature set (routing, auth, roles, flash
  messages, error pages), reimplemented with framework-native equivalents rather than
  hand-rolled ones.
- Be ready to say plainly: the source repo was small. Don't oversell scope you didn't build.

## 2. What was actually broken in the PHP source (say this out loud — it's the most
   interesting part)

- `app/models/User.php` — **completely empty file**.
- `app/controllers/UserController.php` — no class declaration at all, referenced
  `$registrationSuccess` which is never defined anywhere. Not runnable as-is.
- `core/Middleware.php` defined `requireAuth()`/`requireAdmin()` but **nothing in the
  router ever called them** — the protection was dead code.
- `AdminController.dashboard()` rendered `admin/dashboard`, a view that doesn't exist in
  the repo.
- Given this, the login/register/profile/admin flow had to be *reconstructed* from intent
  (`config/routes.php` + `core/Auth.php`), not translated line-by-line. Say this explicitly
  — it's the difference between "converted" and "ported working software."

## 3. Architecture mapping (the core of the explanation)

Walk through this table conversationally, don't read it as a list:

| PHP | .NET | Why |
|---|---|---|
| `Router.php` + `routes.php` (hand-rolled URL table) | ASP.NET Core conventional routing (`{controller=Home}/{action=Index}/{id?}`) | `/user/login` already maps to `UserController.Login` with zero custom routing code — the framework's default *is* the router. |
| `Controller.php` (`view($view, $data)` — `$data` was silently dropped, a real bug) | Plain `Microsoft.AspNetCore.Mvc.Controller`, `View(model)` | The PHP bug (data never reaching the view) doesn't need porting — ASP.NET Core's built-in `View(model)` + strongly-typed `@model` just works. No custom base class needed. |
| `Model.php` (grabs a global `$pdo`) | `AppDbContext : DbContext`, injected via DI | DI replaces the global. |
| `Auth.php` (raw SQL `SELECT ... password_verify`) | `CookieAuthService` using `PasswordHasher<User>` + cookie sign-in | Deliberately **not** full ASP.NET Core Identity — that's 7 tables and a UI scaffold for a one-table, one-role app. `PasswordHasher<T>` alone gives modern PBKDF2 hashing without the machinery. Know this trade-off if asked "why not Identity." |
| `Session.php` (did two jobs: held the user *and* held flash messages) | **No Session middleware at all** | The user identity moved into auth-cookie claims; flash messages moved into TempData. Both PHP jobs are covered by more specific ASP.NET Core features — Session itself became unnecessary. This is a deliberate simplification, not a missing piece. |
| `Middleware.php` (dead code, never wired) | `[Authorize]` / `[Authorize(Policy = "Admin")]` attributes on actions | This is the fix, not just the port: an attribute on the action *can't* be forgotten the way a manual `Middleware::requireAuth()` call was forgotten in the original. |
| `Roles.php` | Authorization policy in `Program.cs`: `RequireClaim(ClaimTypes.Role, "Admin")` | Role is set as a claim at sign-in time. |
| `Notifications.php` (session-backed, read-once flash messages) | `TempData` | TempData's read-once-then-clear behavior is a near-exact match. |
| `Helper.php` (`redirect()`, `sanitizeInput()`) | `RedirectToAction`, Razor's automatic HTML-encoding | Built into the framework; manual sanitization mostly becomes unnecessary because `@Model.Foo` auto-escapes. |
| `Logger.php` (hand-rolled file logger) | `ILogger<T>` via DI | Standard structured logging replaces the custom file writer. |
| `config.php` (hardcoded DB credentials + BASE_URL) | `appsettings.json` (empty placeholder) + `appsettings.Development.json` (SQLite, no secret) + `dotnet user-secrets` for anything real | Nothing is hardcoded. `BASE_URL` mostly disappears — ASP.NET Core generates links via `asp-controller`/`asp-action` tag helpers instead. |

## 4. Data layer

- SQLite instead of MySQL for local dev/demo — no server to install, and there's no
  password to accidentally hardcode. `Program.cs` swaps to `UseMySql` (Pomelo package) in
  one line if MySQL parity is ever needed; the entity model doesn't change.
- `User` entity: `Id`, `Email` (unique index), `PasswordHash`, `Role` (enum stored as
  string). This is the minimal set actually implied by `Auth.php`'s SQL — nothing invented
  beyond what the PHP source referenced (`utenti` table, `email`, `password`, `role`).
- Migrations generated with `dotnet ef migrations add InitialCreate` — standard EF Core
  workflow, applied automatically at startup via `Database.Migrate()`.

## 5. A real bug found during verification (good, honest talking point)

While testing the logout flow end-to-end, `POST /user/logout` returned a confusing
405 instead of redirecting. Root cause: `ErrorController.Handle` was marked `[HttpGet]`
only, but `UseStatusCodePagesWithReExecute` re-executes failed requests **preserving the
original HTTP method** — so a POST that hit an error got re-executed as `POST /error/xxx`,
which didn't match the GET-only error route, producing a second, misleading 405.
Fix: dropped the `[HttpGet]` constraint on the error handler so it accepts any verb.
This is worth mentioning if asked about your testing process — it shows the app was
actually run and exercised, not just compiled.

## 6. Live demo click-through (do this on screen)

1. `/` — home page.
2. `/user/register` — register a new account.
3. `/user/login` — log in with it, land on `/user/profile` showing email + role.
4. Log out — cookie cleared, redirected to login.
5. Try `/user/profile` while logged out — redirected to login with a `ReturnUrl`.
6. Try `/admin/dashboard` while logged out — same redirect (protected by `[Authorize]`).
7. Log in as the seeded admin (`Seed:AdminEmail`/`Seed:AdminPassword` via user-secrets,
   never hardcoded) — visit `/admin/dashboard`, show the user list.
8. Log in as the *non-admin* account, try `/admin/dashboard` — redirected home, not shown
   the page (403-equivalent via the `Admin` policy).
9. Hit a bad URL — custom 404 page, not the framework default.

## 7. Verification (say what you actually ran, not what you assume works)

```
dotnet build                        # 0 warnings, 0 errors (TreatWarningsAsErrors)
dotnet format --verify-no-changes   # clean
dotnet test                         # 6/6 passing (PasswordHasher + AuthService)
dotnet run, then drove the flow above with curl end-to-end
```

## 8. Explicit scope notes

- `config/routes.php` referenced `BlogController` and `PageController` routes that were
  never implemented in the PHP source (commented as "example — doesn't exist yet"). Not
  ported — there was nothing to port.
- No GitHub Actions CI by request — verification is local (`dotnet build`/`test`/`format`).
