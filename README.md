# Sikkerhetsvurderingsplattform for SMB-kunder

Dette repositoryet inneholder et bachelorprosjekt for en API-basert sikkerhetsvurderingsplattform rettet mot små og mellomstore bedrifter. Prosjektet kombinerer en ASP.NET Core-backend med et React-dashboard som vurderer et domene gjennom flere sikkerhetsmoduler og presenterer resultatet i et strukturert grensesnitt for sluttbruker.

## Prosjektsammendrag

Formålet med prosjektet er å undersøke hvordan automatiserte sikkerhetssjekker kan samles inn, tolkes og presenteres på en måte som er forståelig for SMB-kunder. Brukeren skriver inn et domenenavn i dashboardet, og systemet kjører en samlet vurdering basert på SSL/TLS, HTTP security headers, e-postsikkerhet, omdømmedata og indikatorer for post-quantum cryptography readiness.

Applikasjonen er en praktisk bachelorprosjekt-prototype. Den er ikke en erstatning for en profesjonell penetration test, security audit eller compliance assessment.

## Omfang

Den nåværende implementasjonen inkluderer:

- SSL/TLS-analyse basert på data fra SSL Labs, med direkte TLS-probe som fallback
- HTTP security header-analyse basert på direkte header-inspeksjon og Mozilla Observatory-data når det er tilgjengelig
- E-postsikkerhetsanalyse basert på DNS-poster som SPF, DKIM og DMARC
- Omdømmeanalyse basert på DNS-oppslag og VirusTotal-data når en API-nøkkel er konfigurert
- PQC readiness-analyse basert på TLS-protokoll, cipher suites og named groups
- Et samlet assessment-endepunkt som kombinerer modulresultater til score, karakter, status, varsler og en responsmodell tilpasset dashboardet
- Et React-dashboard for å starte en skanning og vise resultater på modulnivå

## Teknologistack

- Backend: ASP.NET Core, C#, .NET 10
- API-dokumentasjon: Swagger / OpenAPI
- Datalagring: Entity Framework Core med in-memory database
- Frontend: React, TypeScript, Vite, Material UI
- Testing: xUnit, ASP.NET Core TestHost, Vitest, Playwright

## Repostruktur

```text
API/
  Controllers/Api/        REST API-kontrollere
  DAL/                    In-memory datakontekst og repositories
  DTOs/                   Request- og responsmodeller
  Services/               Assessment-logikk og eksterne serviceklienter
  global.json             .NET SDK-valg for API-prosjektet

Frontend/
  package.json            Scripts for dashboard-applikasjonen
  dashboard/              React, TypeScript og Vite-dashboard

Test/
  API.UnitTests/          Enhetstester for backend
  API.IntegrationTests/   Smoke- og integrasjonstester for API
  Frontend.UnitTests/     Mapping- og valideringstester for frontend
  E2E/                    Playwright smoke test
  Reports/                Kort testrapport

run-tests.ps1             Kombinert redusert testpakke
package.json              Workspace-script for test:all
```

## Krav

- .NET SDK 10.0.x. API-prosjektet inneholder `API/global.json` med SDK-versjon `10.0.201` og `rollForward` satt til `latestFeature`.
- Node.js som er kompatibel med frontend-verktøyene. Node.js 22 LTS anbefales. Vite-versjonen som brukes i prosjektet krever Node.js `^20.19.0` eller nyere kompatible versjoner.
- npm.
- Internettilgang for live eksterne sjekker, package restore og installasjon av frontend-avhengigheter.

## Konfigurasjon

Backenden kan kjøre uten en lokal konfigurasjonsfil. Omdømmesjekker blir mer fullstendige når en VirusTotal API-nøkkel er konfigurert.

Anbefalt lokal konfigurasjonsfil, dersom VirusTotal skal brukes:

```text
API/appsettings.Local.json
```

Eksempel:

```json
{
  "VirusTotal": {
    "ApiKey": "your-api-key"
  }
}
```

Den samme verdien kan også gis gjennom en miljøvariabel:

```powershell
$env:VirusTotal__ApiKey = "your-api-key"
```

Ikke commit ekte API-nøkler eller lokale secrets.

## Kjøre applikasjonen

Installer frontend-avhengigheter én gang:

```powershell
cd .\Frontend
npm run setup
```

Start utviklingsmiljøet for frontend:

```powershell
npm run dev
```

Dette videresender kommandoen til `Frontend/dashboard`. Dashboardets utviklingsscript starter API-et på `http://localhost:1072` dersom det ikke allerede kjører, og starter deretter Vite frontend på:

```text
http://localhost:5187
```

Frontend proxier `/api`-requests til:

```text
http://localhost:1072
```

Hvis API-et skal kjøre på en annen URL, kan `VITE_DEV_API_PROXY` settes i `Frontend/dashboard/.env.development`.

## Kjøre API separat

Fra repository root:

```powershell
dotnet run --project .\API\SecurityAssessmentAPI.csproj --launch-profile http
```

Eller fra `API`-mappen:

```powershell
dotnet run --project .\SecurityAssessmentAPI.csproj --launch-profile http
```

Standard API-URL-er:

```text
API:          http://localhost:1072
Swagger UI:   http://localhost:1072/swagger
OpenAPI JSON: http://localhost:1072/swagger/v1/swagger.json
```

## Viktigste API-endepunkter

```text
GET /api/assessment/check/{domain}
GET /api/ssl/check/{domain}
GET /api/ssl/details/{domain}
GET /api/headers/check/{domain}
GET /api/email/check/{domain}
GET /api/reputation/check/{domain}
GET /api/pqc/check/{domain}
GET /
```

## Testing

Installer testavhengigheter før frontend- og E2E-tester kjøres:

```powershell
cd .\Test\Frontend.UnitTests
npm ci

cd ..\E2E
npm ci
npx playwright install chromium
```

Kjør den komplette, reduserte testpakken fra repository root:

```powershell
.\run-tests.ps1
```

Eller bruk npm shortcut fra repository root:

```powershell
npm run test:all
```

Den samme samlede testen kan også kjøres fra `Test`-mappen med `npm run test:all`.

Den kombinerte testpakken dekker:

- 55 enhetstester for backend
- 3 integrasjons-/smoke-tester for backend
- 14 enhetstester for frontend
- 1 Playwright E2E smoke test for hovedflyten for skanning

Teststrategien er bevisst redusert for levering. Den beskytter sentral scoring-logikk, API-kontraktens struktur, frontend mapping/validation og hovedflyten for skanning.

## Eksterne tjenester og datakvalitet

Applikasjonen er avhengig av eksterne systemer og nettverksforhold:

- SSL Labs for TLS- og sertifikatanalyse
- Mozilla Observatory når tilgjengelig for HTTP header-vurdering
- DNS-oppslag for e-post- og domeneoppløsning
- VirusTotal for omdømmedata når API-nøkkel er konfigurert
- Direkte HTTP-requests for header probing

Fordi disse kildene er eksterne, kan resultatene variere over tid og påvirkes av rate limits, nettverkstilgjengelighet, nedetid hos leverandører eller ufullstendige upstream-data.

## Begrensninger

- Databasen er in-memory og er ikke ment for persistent production storage.
- Scoringmodellen er en bachelorprosjekt-implementasjon og bør tolkes som en prototype.
- En clean eller high score beviser ikke at et domene er sikkert.
- Manglende eksterne data håndteres defensivt, men det kan redusere presisjonen i vurderingen.
- Prosjektet fokuserer på et utvalg observerbare indikatorer på domenenivå og undersøker ikke intern infrastruktur, applikasjonens kildekode, identity systems eller organisatoriske prosesser.

## Leveransenotater

Genererte avhengigheter og build outputs er bevisst ekskludert fra repositoryet og bør ikke inkluderes i delivery zip files:

```text
.git/
.vs/
.vscode/
.idea/
.dotnet-cli-home/
artifacts/
**/.artifacts/
**/bin/
**/obj/
**/node_modules/
**/dist/
**/test-results/
**/playwright-report/
coverage/
TestResults/
*.log
*.tmp
*.temp
```

Package lock files bør forbli i repositoryet. De gjør npm-installasjonen mer reproducerbar, samtidig som mottaker kan installere avhengigheter lokalt.

## Akademisk kontekst

For bachelorprosjektlevering er denne README-en ment å dokumentere:

- hva systemet gjør
- hvordan kildekoden er organisert
- hvordan applikasjonen kan konfigureres og kjøres
- hvordan den reduserte validation-testpakken kan kjøres
- hvilke antakelser og begrensninger som gjelder

Detaljert teoretisk bakgrunn, metode, diskusjon og evaluering bør fortsatt ligge i bacheloroppgaven eller prosjektrapporten. Denne README-en fokuserer på reproducerbarhet og teknisk overlevering.
