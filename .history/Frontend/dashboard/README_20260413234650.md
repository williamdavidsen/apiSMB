# Security dashboard (frontend)

React + TypeScript + Vite app: domain scan flow, assessment summary, and module cards are backed by the ASP.NET API (in development, `/api` is proxied via Vite).

## Running locally

1. Install **Node.js** (LTS recommended) and **npm**.
2. From the repository root, go to `Frontend/dashboard`.
3. Install dependencies:

   ```bash
   npm install
   ```

4. Start the **API** (default dev target is `http://localhost:5052`). Run the `API` project from this repo; Vite forwards `/api` requests to that base URL.
5. Start the dev server:

   ```bash
   npm run dev
   ```

   The app is usually served at `http://localhost:5173`.

If the API runs on another host or port, add `.env.development` in this folder and set `VITE_DEV_API_PROXY` to the full base URL (e.g. `http://localhost:5052`).

## Scripts

| Command           | Description                          |
| ----------------- | ------------------------------------ |
| `npm run dev`     | Dev server with HMR                  |
| `npm run build`   | Typecheck + production bundle (`dist/`) |
| `npm run preview` | Preview the production build locally |
| `npm run lint`    | ESLint                               |

## Stack

Vite, React 19, MUI v9, React Router. See `eslint.config.js` and `tsconfig.*` for lint and TypeScript setup.

---

This project was bootstrapped with Vite. Official React plugin options: [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react) (Oxc), [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) (SWC).
