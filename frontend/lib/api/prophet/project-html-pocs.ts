import { prophetDel, prophetGet, prophetPost } from "@/lib/api/client"
import type { ApiResult } from "@/lib/api/types"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"
import { reportRequestError } from "@/lib/api/request-status/request-status"

/** Matches Prophet API JSON enum names (`Web`, `Mobile`). */
export type ProphetPocKind = "Web" | "Mobile"

export type ProphetProjectHtmlPocItemDto = {
  id: string
  kind: ProphetPocKind
  originalFileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

export type ProphetProjectHtmlPocUrlDto = {
  downloadUrl: string
}

export type ProphetProjectHtmlPocContentDto = {
  text: string
}

export type UploadProphetHtmlPocResult =
  | { ok: true; document: ProphetProjectHtmlPocItemDto }
  | { ok: false; conflict: true }

function normalizeHtmlPocItem(
  raw: ProphetProjectHtmlPocItemDto & { kind?: unknown }
): ProphetProjectHtmlPocItemDto {
  const k = raw.kind as string | number | undefined
  const kind: ProphetPocKind =
    k === "Web" || k === 0
      ? "Web"
      : k === "Mobile" || k === 1
        ? "Mobile"
        : "Web"
  return { ...raw, kind }
}

export async function listProphetProjectHtmlPocs(
  projectId: string
): Promise<ProphetProjectHtmlPocItemDto[]> {
  const res = await prophetGet(`/v1/prophet/projects/${projectId}/html-pocs`)
  const json =
    await readProphetApiResultJson<ProphetProjectHtmlPocItemDto[]>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list HTML POCs"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.map((row) =>
    normalizeHtmlPocItem(
      row as ProphetProjectHtmlPocItemDto & { kind?: unknown }
    )
  )
}

export async function uploadProphetProjectHtmlPoc(
  projectId: string,
  kind: ProphetPocKind,
  file: File,
  replaceConfirmed: boolean
): Promise<UploadProphetHtmlPocResult> {
  const form = new FormData()
  form.append("kind", kind)
  form.append("replaceConfirmed", replaceConfirmed ? "true" : "false")
  form.append("file", file)

  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/html-pocs`,
    form
  )
  const text = await res.text()
  const trimmed = text.trim()
  if (!trimmed) {
    const message = `HTTP ${res.status}`
    reportRequestError(message)
    throw new Error(message)
  }
  let json: ApiResult<ProphetProjectHtmlPocItemDto>
  try {
    json = JSON.parse(trimmed) as ApiResult<ProphetProjectHtmlPocItemDto>
  } catch {
    const message = `HTTP ${res.status}: response is not JSON`
    reportRequestError(message)
    throw new Error(message)
  }

  if (
    res.status === 409 &&
    json.messages?.some((m) => m.includes("poc_kind_already_exists"))
  ) {
    return { ok: false, conflict: true }
  }

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to upload"
    reportRequestError(message)
    throw new Error(message)
  }

  return {
    ok: true,
    document: normalizeHtmlPocItem(
      json.data as ProphetProjectHtmlPocItemDto & { kind?: unknown }
    ),
  }
}

export async function deleteProphetProjectHtmlPoc(
  projectId: string,
  documentId: string
): Promise<void> {
  const res = await prophetDel(
    `/v1/prophet/projects/${projectId}/html-pocs/${documentId}`
  )
  const json = await readProphetApiResultJson<boolean>(res)
  if (!res.ok || !json.success) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to delete"
    reportRequestError(message)
    throw new Error(message)
  }
}

export async function getProphetProjectHtmlPocDownloadUrl(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/html-pocs/${documentId}/download`
  )
  const json = await readProphetApiResultJson<ProphetProjectHtmlPocUrlDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to get download URL"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.downloadUrl
}

/** Signed URL intended for opening in a new tab (inline HTML, no forced download). */
export async function getProphetProjectHtmlPocViewUrl(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/html-pocs/${documentId}/view`
  )
  const json = await readProphetApiResultJson<ProphetProjectHtmlPocUrlDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to get view URL"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.downloadUrl
}

/** HTML body for popup preview (API proxy; avoids browser CORS on GCS signed GET). */
export async function getProphetProjectHtmlPocContent(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/html-pocs/${documentId}/content`
  )
  const json =
    await readProphetApiResultJson<ProphetProjectHtmlPocContentDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to load HTML POC"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.text
}

/** @deprecated Same as {@link getProphetProjectHtmlPocContent} — name was a common typo (`context` vs `content`). */
export const getProphetProjectHtmlPocContext = getProphetProjectHtmlPocContent
