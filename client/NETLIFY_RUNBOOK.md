# Netlify Deploy Runbook (short)

1) Build settings

- Build command: `pnpm --filter client run build`
- Publish directory: `build/public`

2) Required environment variables (set in Netlify site Settings → Build & deploy → Environment)

- `API_BASE_URL` — the full URL to your API (e.g. `https://api.zap.example.com`). If you host API on the same domain, you may leave this empty and the client will use same-origin.
- `SESSION_SECRET` — a strong random secret used by Remix cookie sessions. Example generation: `openssl rand -base64 32` or use your secrets manager.
- `NODE_ENV` — set to `production` (Netlify usually sets this automatically).

3) Notes

- If your API is behind authentication or requires CORS, ensure the API allows requests from your Netlify site origin.
- For server-side functions (if you use them), ensure `API_BASE_URL` is reachable from the serverless runtime.

4) Post-deploy verification

- Visit the site URL and perform a login / create company flow to verify API connectivity.
- If you get session/cookie errors, verify `SESSION_SECRET` is set and the domain/path for cookies aligns with your hosting setup.
