# Frontend

Quick summary:

- Enter `Frontend`
- Run `npm run setup` once
- Run `npm run dev`

Command:

```powershell
cd .\Frontend
npm run setup
npm run dev
```

What this does:

- The `Frontend/package.json` `dev` script forwards to `Frontend/dashboard`
- The `Frontend/package.json` `setup` script installs dependencies in `Frontend/dashboard`
- Vite starts the frontend development server
- The app usually opens on `http://localhost:5173`
- In development, `/api` is proxied to the backend at `http://localhost:1071`

If you want to work directly inside the dashboard app:

```powershell
cd .\Frontend\dashboard
npm install
npm run dev
```
