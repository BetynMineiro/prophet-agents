/**
 * Prophet frontend configuration.
 * Reads NEXT_PUBLIC_PROPHET_API_URL (no trailing slash).
 */

export function getProphetBaseUrl(): string {
  return (
    process.env.NEXT_PUBLIC_PROPHET_API_URL?.trim().replace(/\/$/, "") ??
    "https://localhost:7017"
  )
}
