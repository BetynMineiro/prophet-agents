/**
 * Simplified HTTP client for the Prophet API (no authentication).
 * Base URL from NEXT_PUBLIC_PROPHET_API_URL.
 */

import { getProphetBaseUrl } from "@/lib/core/config"
import {
  clearRequestError,
  reportRequestError,
  requestFinished,
  requestStarted,
} from "@/lib/api/request-status/request-status"

export interface RequestOptions extends Omit<RequestInit, "body"> {
  /** Request body (object will be JSON.stringify'd) */
  body?: unknown
}

function buildUrl(base: string, path: string): string {
  const b = base.trim().replace(/\/$/, "")
  const normalizedPath = path.startsWith("/") ? path : `/${path}`
  return b ? `${b}${normalizedPath}` : normalizedPath
}

function serializeBody(body: unknown): BodyInit | undefined {
  if (body === undefined || body === null) return undefined
  if (typeof body === "string") return body
  if (body instanceof FormData) return body
  return JSON.stringify(body)
}

async function requestWithBase(
  getBase: () => string,
  path: string,
  options: RequestOptions = {}
): Promise<Response> {
  requestStarted()
  const { body, headers = {}, ...rest } = options
  const url = buildUrl(getBase(), path)

  const h: Record<string, string> = {
    ...(headers as Record<string, string>),
  }
  if (!(body instanceof FormData)) h["Content-Type"] = "application/json"

  try {
    clearRequestError()
    const res = await fetch(url, {
      ...rest,
      headers: h,
      body: serializeBody(body),
    })
    return res
  } catch (e) {
    reportRequestError(e)
    throw e
  } finally {
    requestFinished()
  }
}

/** GET (Prophet API) */
export function prophetGet(
  path: string,
  options: RequestOptions = {}
): Promise<Response> {
  return requestWithBase(getProphetBaseUrl, path, { ...options, method: "GET" })
}

/** POST (Prophet API) */
export function prophetPost(
  path: string,
  body?: unknown,
  options: RequestOptions = {}
): Promise<Response> {
  return requestWithBase(getProphetBaseUrl, path, {
    ...options,
    method: "POST",
    body,
  })
}

/** PUT (Prophet API) */
export function prophetPut(
  path: string,
  body?: unknown,
  options: RequestOptions = {}
): Promise<Response> {
  return requestWithBase(getProphetBaseUrl, path, {
    ...options,
    method: "PUT",
    body,
  })
}

/** PATCH (Prophet API) */
export function prophetPatch(
  path: string,
  body?: unknown,
  options: RequestOptions = {}
): Promise<Response> {
  return requestWithBase(getProphetBaseUrl, path, {
    ...options,
    method: "PATCH",
    body,
  })
}

/** DELETE (Prophet API) */
export function prophetDel(
  path: string,
  options: RequestOptions = {}
): Promise<Response> {
  return requestWithBase(getProphetBaseUrl, path, {
    ...options,
    method: "DELETE",
  })
}
