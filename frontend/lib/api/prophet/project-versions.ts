import { prophetGet, prophetPost } from "@/lib/api/client"
import type { CursorPage } from "@/lib/api/types"
import { reportRequestError } from "@/lib/api/request-status/request-status"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"

export type ProphetArtifactVersionItemDto = {
  id: string
  pipelineProjectId: string
  versionNumber: number
  parentVersionId: string | null
  changeSummary: string | null
  pipelineStatus: string
  currentStepIndex: number
  totalSteps: number
  createdAtUtc: string
}

export async function listProphetProjectArtifactVersions(params: {
  projectId: string
  pageSize?: number
  cursor?: string | null
}): Promise<CursorPage<ProphetArtifactVersionItemDto>> {
  const { projectId, pageSize = 20, cursor = null } = params
  const q = new URLSearchParams()
  q.set("pageSize", String(pageSize))
  if (cursor) q.set("cursor", cursor)

  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions?${q.toString()}`
  )
  const json =
    await readProphetApiResultJson<CursorPage<ProphetArtifactVersionItemDto>>(
      res
    )
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list artifact versions"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export type CreateProphetArtifactVersionPayload = {
  parentVersionId?: string | null
  changeSummary?: string | null
}

/** POST /v1/prophet/projects/{projectId}/versions — creates a new artifact version (e.g. when the project has inputs but no version yet). */
export async function createProphetArtifactVersion(
  projectId: string,
  payload: CreateProphetArtifactVersionPayload = {}
): Promise<ProphetArtifactVersionItemDto> {
  const body = {
    parentVersionId: payload.parentVersionId ?? null,
    changeSummary: payload.changeSummary ?? null,
  }
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/versions`,
    body
  )
  const json =
    await readProphetApiResultJson<ProphetArtifactVersionItemDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to create artifact version"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}
