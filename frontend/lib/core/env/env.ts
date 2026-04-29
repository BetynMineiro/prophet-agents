/**
 * Environment helpers for SSR-safe checks.
 * Use these instead of ad-hoc window/process checks.
 */

/** True when running in a browser (window is defined). Use for redirects, localStorage, etc. */
export function isClient(): boolean {
  return globalThis.window !== undefined
}
