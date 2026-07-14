# MvcBase (.NET port)

ASP.NET Core MVC (.NET 9) port of [Roberto-Celano/MVCbase](https://github.com/Roberto-Celano/MVCbase),
a small PHP MVC teaching skeleton. See `docs/NARRATION.md` for a full architecture walkthrough
and the PHP-to-.NET mapping.

## Run locally

```bash
dotnet restore
dotnet ef database update --project src/MvcBase.Web
dotnet run --project src/MvcBase.Web
```

Optional: seed an admin account by setting user-secrets before first run:

```bash
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project src/MvcBase.Web
dotnet user-secrets set "Seed:AdminPassword" "<choose-a-password>" --project src/MvcBase.Web
```

## Verify

```bash
dotnet build
dotnet format --verify-no-changes
dotnet test
```

## Scope notes

`config/routes.php` in the original repo referenced `BlogController` and `PageController`
routes that were never implemented in the source — those are intentionally not ported.
