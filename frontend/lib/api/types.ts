/**
 * Prophet API response envelope.
 * All responses have success, statusCode, messages (on error), data (on success).
 */

export interface ApiResult<T = unknown> {
  success: boolean
  statusCode: number
  messages?: string[]
  data?: T
}

export interface CursorPage<T> {
  items: T[]
  nextCursor: string | null
  hasNext: boolean
}
