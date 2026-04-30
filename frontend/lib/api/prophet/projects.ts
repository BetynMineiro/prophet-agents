import {
  prophetDel,
  prophetGet,
  prophetPatch,
  prophetPost,
  prophetPut,
} from "@/lib/api/client"
import type { ApiResult } from "@/lib/api/types"
import { reportRequestError } from "@/lib/api/request-status/request-status"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"
import {
  ActiveState as ActiveStateValue,
  type ActiveState,
} from "@/lib/api/active-state"

export type ProphetProjectItemDto = {
  id: string
  name: string
  description: string | null
  /** ISO date `YYYY-MM-DD`, or null if not set */
  expectedDate: string | null
  createdAtUtc: string
  isActive: boolean
  /** Highest artifact version id for this project; null if no versions */
  latestArtifactVersionId?: string | null
  /**
   * Pipeline status for that version (Genesis: Idle, Running, Completed, Failed, Paused).
   * Null when there are no artifact versions.
   */
  latestPipelineStatus?: string | null
}

export type CreateProphetProjectPayload = {
  name: string
  description: string | null
  expectedDate: string | null
  isActive: boolean
}

export type UpdateProphetProjectPayload = {
  name: string
  description: string | null
  expectedDate: string | null
  isActive: boolean
}

export async function getProphetProjects(params?: {
  searchText?: string | null
  activeState?: ActiveState
}): Promise<ProphetProjectItemDto[]> {
  const { searchText, activeState = ActiveStateValue.Active } = params ?? {}
  const q = new URLSearchParams()
  if (searchText) q.set("searchText", searchText)
  q.set("activeState", activeState)

  const res = await prophetGet(`/v1/prophet/projects?${q.toString()}`)
  const json = await readProphetApiResultJson<ProphetProjectItemDto[]>(res)

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to fetch"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}

export async function getProphetProject(
  projectId: string
): Promise<ProphetProjectItemDto> {
  const res = await prophetGet(`/v1/prophet/projects/${projectId}`)
  const json = await readProphetApiResultJson<ProphetProjectItemDto>(res)

  if (res.status === 404 || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      (res.status === 404 ? "Not found" : `HTTP ${res.status}`) ||
      "Failed to fetch"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}

export async function createProphetProject(
  payload: CreateProphetProjectPayload
): Promise<ProphetProjectItemDto> {
  const res = await prophetPost("/v1/prophet/projects", payload)
  const json = await readProphetApiResultJson<ProphetProjectItemDto>(res)

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to create"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}

export async function updateProphetProject(
  projectId: string,
  payload: UpdateProphetProjectPayload
): Promise<ProphetProjectItemDto> {
  const res = await prophetPut(`/v1/prophet/projects/${projectId}`, payload)
  const json = await readProphetApiResultJson<ProphetProjectItemDto>(res)

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to update"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}

export async function deleteProphetProject(
  projectId: string
): Promise<ApiResult<boolean>> {
  const res = await prophetDel(`/v1/prophet/projects/${projectId}`)
  const json = await readProphetApiResultJson<boolean>(res)
  return json
}

export async function restoreProphetProject(
  projectId: string
): Promise<ProphetProjectItemDto> {
  const res = await prophetPatch(`/v1/prophet/projects/${projectId}/restore`)
  const json = await readProphetApiResultJson<ProphetProjectItemDto>(res)

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to restore"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}

export type RefineProphetProjectPayload = {
  change_request: string
}

export type RefineProphetProjectResultDto = {
  versionId: string
  versionNumber: number
  startFromStepIndex: number
}

export async function refineProphetProject(
  projectId: string,
  payload: RefineProphetProjectPayload
): Promise<RefineProphetProjectResultDto> {
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/refine`,
    payload
  )
  const json =
    await readProphetApiResultJson<RefineProphetProjectResultDto>(res)

  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") || `HTTP ${res.status}` || "Failed to refine"
    reportRequestError(message)
    throw new Error(message)
  }

  return json.data
}
