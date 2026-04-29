import { prophetDel, prophetGet, prophetPost } from "@/lib/api/client"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"
import { reportRequestError } from "@/lib/api/request-status/request-status"

export type ProphetProjectInputDocumentItemDto = {
  id: string
  originalFileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

export type ProphetProjectInputUploadItemResult = {
  fileName: string
  success: boolean
  errorMessage: string | null
  document: ProphetProjectInputDocumentItemDto | null
}

export type UploadProphetProjectInputsResponse = {
  results: ProphetProjectInputUploadItemResult[]
}

export type ProphetProjectInputDownloadDto = {
  downloadUrl: string
}

export async function listProphetProjectInputs(
  projectId: string
): Promise<ProphetProjectInputDocumentItemDto[]> {
  const res = await prophetGet(`/v1/prophet/projects/${projectId}/inputs`)
  const json =
    await readProphetApiResultJson<ProphetProjectInputDocumentItemDto[]>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list inputs"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function uploadProphetProjectInputs(
  projectId: string,
  files: File[]
): Promise<UploadProphetProjectInputsResponse> {
  const form = new FormData()
  for (const f of files) {
    form.append("files", f)
  }
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/inputs`,
    form
  )
  const json =
    await readProphetApiResultJson<UploadProphetProjectInputsResponse>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to upload"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function deleteProphetProjectInput(
  projectId: string,
  documentId: string
): Promise<void> {
  const res = await prophetDel(
    `/v1/prophet/projects/${projectId}/inputs/${documentId}`
  )
  const json = await readProphetApiResultJson<boolean>(res)
  if (!res.ok || !json.success) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to delete"
    reportRequestError(message)
    throw new Error(message)
  }
}

export async function getProphetProjectInputDownloadUrl(
  projectId: string,
  documentId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/inputs/${documentId}/download`
  )
  const json =
    await readProphetApiResultJson<ProphetProjectInputDownloadDto>(res)
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
