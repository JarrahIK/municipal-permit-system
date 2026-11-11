# Municipal Building Permit System — Backend API

Smart municipal platform that digitizes the building-permit lifecycle for Jordanian municipalities:
automated fee calculation, document handling and a multi-role approval workflow
(Citizen → Surveyor → Department Head → Manager → Accountant).

**Status: Phase 0 (foundations).** Solution structure, database schema, authentication (citizen +
separate staff login), Docker and CI/CD are in place. Fee engine, workflows, payments and the AI
assistant arrive in later phases.

## Tech stack

| Layer | Choice |
|---|---|
| API | ASP.NET Core 10 Web API (C#) |
| API docs | Built-in OpenAPI + Scalar (UI at `/scalar`, development only) |
| Database | SQL Server (LocalDB while developing, a SQL Server container in Docker) |
| Data access | Entity Framework Core 10 (migrations) |
| Auth | JWT bearer tokens, bcrypt password hashing, role-based access control |
| Field encryption | AES-256-GCM (phone numbers and addresses are encrypted in the database) |
| Tests | xUnit |
| Containers / CI/CD | Docker, GitHub Actions, GitHub Container Registry |
| Frontend (separate) | React |

## Layout

```
src/MunicipalPermitSystem.Api/
  Program.cs                   start-up: settings, database, auth, CORS, docs
  Controllers/AuthController   register / login / staff login / "who am I"
  Services/AuthService         login logic + the 5-fails-locks-15-minutes rule
  Services/…                   bcrypt hasher, JWT creator, AES-GCM encryptor, account store
  Models/Entities, Models/Enums   the tables from your ER / class diagrams
  Data/ApplicationDbContext    how the entities map to tables
  Data/DevDataSeeder           demo accounts (Development only)
  Configuration/               settings classes + "refuse to start with insecure secrets"
tests/MunicipalPermitSystem.Tests/
Dockerfile, docker-compose*.yml, .env*.example
.github/workflows/main.yml     the CI/CD pipeline
```

## Run it in Visual Studio

Needs the .NET 10 SDK and SQL Server **LocalDB** (installed with Visual Studio's web workload;
check with `sqllocaldb info` — it should list `MSSQLLocalDB`).

1. Open `MunicipalPermitSystem.sln`. Right-click **MunicipalPermitSystem.Api** → *Set as Startup Project*.
2. **Create the first migration — once.** *Tools → NuGet Package Manager → Package Manager Console*,
   set *Default project* to `MunicipalPermitSystem.Api`, then run:
   ```
   Add-Migration InitialCreate
   ```
   This creates a `Migrations` folder. **Commit it** — CI and Docker need it.
3. Press **F5** (profile `http`). On start-up the app creates the database, applies the migration and
   adds demo accounts. Your browser opens the Scalar API docs at <http://localhost:5080/scalar>.
4. Try it: open `MunicipalPermitSystem.Api.http` (next to `Program.cs`) and click *Send request*
   on the staff-login request, then on "Who am I?". The token is passed along automatically.

| Demo account (password `DevPass123!`) | ID | Log in with |
|---|---|---|
| Surveyor | `SURV001` | `POST /api/auth/staff/login` |
| Department Head | `HEAD001` | `POST /api/auth/staff/login` |
| Manager | `MGR001` | `POST /api/auth/staff/login` |
| Accountant | `ACC001` | `POST /api/auth/staff/login` |
| Citizen | `1000000001` | `POST /api/auth/citizen/login` |

Changing the entities later: `Add-Migration SomeName` — the next run applies it.
Starting over with an empty database: `Drop-Database` in the Package Manager Console, then F5.

## Tests

*Test → Test Explorer → Run All Tests*, or `dotnet test`.

- Login / lockout / encryption / startup-validation / HTTP status tests need no database.
- `DatabaseModelTests` check the table mapping on a throw-away in-memory SQLite database.

## Docker

Docker packs the app and everything it needs into a "container" that runs the same on any computer.
You do **not** need it while coding — it's for the production-style run and the final demo.

```powershell
copy .env.example .env                       # then edit the passwords if you like
docker compose up --build                    # development (API at http://localhost:8080/scalar)
```

Production-configured run, using the image the pipeline published (see next section):

```powershell
copy .env.prod.example .env.prod             # fill in REAL secrets
docker compose -p permit-prod --env-file .env.prod -f docker-compose.yml -f docker-compose.prod.yml pull api
docker compose -p permit-prod --env-file .env.prod -f docker-compose.yml -f docker-compose.prod.yml up -d --no-build
```

Generating secrets (PowerShell 7+; or `openssl rand -base64 48` / `32` in Git Bash):
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))   # JWT_KEY
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))   # ENCRYPTION_KEY
```

## Pushing to GitHub and the CI/CD pipeline

One-time setup — create an **empty** repository on github.com (no README), then in the project folder:

```powershell
git init -b main
git add .
git commit -m "Phase 0: ASP.NET Core foundation"
git remote add origin https://github.com/JarrahIK/municipal-permit-system.git
git push -u origin main
git switch -c develop
git push -u origin develop
```

What happens on every push (`.github/workflows/main.yml`):

```
git push ──► GitHub Actions
              ├─ build-and-test : restore → build → run all tests        (a failure stops everything)
              └─ docker         : build the Docker image
                                  └─ push to ghcr.io  (main / develop / version tags only)
```

| You push to | Image published | Meant for |
|---|---|---|
| a feature branch / pull request | nothing (only proves it builds) | review |
| `develop` | `ghcr.io/<you>/municipal-permit-system:develop` | development / pre-production |
| `main` | `:main`, `:latest`, `:sha-<commit>` | production |
| tag `v1.0.0` | `:1.0.0` | a numbered release |

Branches: work on `feature/…` branches → pull request into `develop` → when stable, pull request
`develop` into `main`. Use GitHub **Issues** and a **Projects** board for tasks (the rubric asks for them).

Run the published image: log in once (`docker login ghcr.io -u <you>` with a token that has
`read:packages`; not needed if you make the package public in GitHub → Packages → settings), then use
the production commands above.

## Development vs. production

Same code and image in both; only the environment differs.

| | Development | Production |
|---|---|---|
| Run with | F5 (LocalDB) or `docker compose up` | `docker compose … -f docker-compose.prod.yml` |
| Database | `MunicipalPermitDb_Dev` | `MunicipalPermitDb_Prod`, port not published |
| API docs (`/scalar`) | on | **off** |
| Demo accounts | created | **refused** (app won't start if enabled) |
| Secrets | insecure built-in dev values | app **refuses to start** with those values |
| Settings from | `appsettings.Development.json` | environment variables (`.env.prod`) |
| Image | built locally | pulled from the registry |

## API (current)

| Method & path | Purpose |
|---|---|
| `POST /api/auth/citizen/register` | FR1.2 — register with a National ID |
| `POST /api/auth/citizen/login` | FR1.3 |
| `POST /api/auth/staff/login` | Staff login (separate from citizens, same internal auth — not third-party SSO) |
| `POST /api/auth/citizen/forgot-password` | FR1.4 — answers 501 until the recovery channel is decided |
| `GET /api/auth/me` | Who the token says you are (needs a token) |
| `GET /api/health` | Alive + database reachable |

Security behaviour: an account locks for 15 minutes after 5 failed logins (HTTP 423); passwords are
bcrypt-hashed; phone numbers and addresses are AES-256-GCM encrypted at rest; "unknown ID" and
"wrong password" look identical to the caller. Use `[Authorize(Roles = AppRoles.Surveyor)]` etc. to
protect future endpoints.

## Deliberately mocked

- **Civil Status check (SFR6)** — will be a local stand-in, not a real government integration.
- **Payments** — no gateway: confirming a payment flips a status and creates a receipt; a hash of each
  finalized transaction is kept as the audit-log ("blockchain") entry (`Payment.BlockchainHash`).

## Not built yet

Fee and penalty engine, document upload (Phase 1) · role workflows and dashboards (Phase 2) ·
mocked payment + audit hash (Phase 3) · AI assistant (Phase 4) · complaints and ratings (Phase 5) ·
password reset (FR1.4) · a way to create staff accounts outside Development.

## Open decisions

1. **National ID encryption (NFR1.2).** The National ID is also the login key, so encrypting it breaks
   lookups unless we add a keyed-hash column. It is stored in plain text for now; phone and address are encrypted.
2. **`CommercialPermit.NumberOfFloors`** is in the class diagram but not the ER diagram — kept optional.
3. **IDs** — permit, payment and document IDs are integers (the ER diagram used `varchar(50)`).
4. **Password recovery channel** (email or SMS) for FR1.4.
5. **HTTPS (NFR1.3)** — in production TLS is expected to be provided in front of the API
   (reverse proxy / hosting). Set `Https:Redirect=true` if the API itself should force HTTPS.
6. **Creating real staff accounts** — no admin tool yet; only the Development demo accounts exist.
