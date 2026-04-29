import type { ApiResult } from "@/lib/api/types"
import { reportRequestError } from "@/lib/api/request-status/request-status"

/** Parses Result JSON; avoids throw on empty 404/HTML bodies (NotFound() often has no JSON). */
export async function readProphetApiResultJson<T>(
  res: Response
): Promise<ApiResult<T>> {
  const text = await res.text()
  const trimmed = text.trim()
  if (!trimmed) {
    const message = `HTTP ${res.status}`
    reportRequestError(message)
    throw new Error(message)
  }
  try {
    return JSON.parse(trimmed) as ApiResult<T>
  } catch {
    const message = `HTTP ${res.status}: response is not JSON`
    reportRequestError(message)
    throw new Error(message)
  }
}
