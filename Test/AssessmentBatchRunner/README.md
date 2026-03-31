# Assessment Batch Runner

Bu arac lokal calisan API'ye toplu domain istekleri gonderir.

## Varsayilanlar

- Base URL: `http://localhost:1111`
- Endpoint: `/api/assessment/check/{domain}`
- Domain listesi: `domains.txt`

## Calistirma

API calisir durumda olsun. Ardindan bu klasorde:

```powershell
dotnet run
```

Farkli base URL veya domain dosyasi vermek icin:

```powershell
dotnet run -- http://localhost:1111 .\domains.txt
```

## Cikti

Calisma sonunda `output` klasorune iki dosya yazar:

- `assessment-results-<timestamp>.json`
- `assessment-results-<timestamp>.csv`

CSV ozet kolonlari:

- overall score
- final status
- grade
- ssl status
- headers status
- email status
- reputation status
- pqc status
- alert types
- alert messages
