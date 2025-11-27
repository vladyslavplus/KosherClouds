/// <reference types="vite/client" />
/// <reference types="vite-plugin-svgr/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_API_TIMEOUT: string;
  readonly VITE_STRIPE_PUBLIC_KEY: string;
  readonly VITE_APP_NAME: string;
  readonly VITE_APP_ENV: string;
  readonly VITE_FEATURE_REVIEWS: string;
  readonly VITE_FEATURE_BOOKINGS: string;
  readonly VITE_FEATURE_PAYMENTS: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}