import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

const UUID_RE =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

/** Returns true for canonical UUID strings (incl. v7-style IDs used by the API). */
export function isUuid(value: string): boolean {
  return UUID_RE.test(value.trim())
}

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
