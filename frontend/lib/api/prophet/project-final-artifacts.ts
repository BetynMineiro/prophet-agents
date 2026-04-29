import { prophetDel, prophetGet, prophetPost } from "@/lib/api/client"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"
import { reportRequestError } from "@/lib/api/request-status/request-status"

export type ProphetProjectFinalArtifactItemDto = {
  id: string
  originalFileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

export type ProphetProjectFinalArtifactUploadItemResult = {
  fileName: string
  success: boolean
  errorMessage: string | null
  document: ProphetProjectFinalArtifactItemDto | null
}

export type UploadProphetProjectFinalArtifactsResponse = {
  results: ProphetProjectFinalArtifactUploadItemResult[]
}

export type ProphetProjectFinalArtifactDownloadDto = {
  downloadUrl: string
}

export type ProphetProjectFinalArtifactContentDto = {
  text: string
}

export async function listProphetProjectFinalArtifacts(
  projectId: string
): Promise<ProphetProjectFinalArtifactItemDto[]> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/final-artifacts`
  )
  const json =
    await readProphetApiResultJson<ProphetProjectFinalArtifactItemDto[]>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list final artifacts"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function uploadProphetProjectFinalArtifacts(
  projectId: string,
  files: File[]
): Promise<UploadProphetProjectFinalArtifactsResponse> {
  const form = new FormData()
  for (const f of files) {
    form.append("files", f)
  }
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/final-artifacts`,
    form
  )
  const json =
    await readProphetApiResultJson<UploadProphetProjectFinalArtifactsResponse>(
      res
    )
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to upload"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function deleteProphetProjectFinalArtifact(
  projectId: string,
  documentId: string
): Promise<void> {
  const res = await prophetDel(
    `/v1/prophet/projects/${projectId}/final-artifacts/${documentId}`
  )
  const json = await readProphetApiResultJson<boolean>(res)
  if (!res.ok || !json.success) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to delete"
    reportRequestError(message)
    throw new Error(message)
  }
}

export async function getProphetProjectFinalArtifactDownloadUrl(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/final-artifacts/${documentId}/download`
  )
  const json =
    await readProphetApiResultJson<ProphetProjectFinalArtifactDownloadDto>(res)
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

export async function getProphetProjectFinalArtifactContent(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/final-artifacts/${documentId}/content`
  )
  const json =
    await readProphetApiResultJson<ProphetProjectFinalArtifactContentDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to load artifact content"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.text
}

/** @deprecated Same as {@link getProphetProjectFinalArtifactContent} — name was a common typo (`context` vs `content`). */
export const getProphetProjectFinalArtifactContext =
  getProphetProjectFinalArtifactContent
