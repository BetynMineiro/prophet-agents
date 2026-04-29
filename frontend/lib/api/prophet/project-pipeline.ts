import { prophetGet, prophetPost } from "@/lib/api/client"
import { reportRequestError } from "@/lib/api/request-status/request-status"
import { readProphetApiResultJson } from "@/lib/api/prophet/parse-prophet-result"

export type ProphetPipelineStepStatusDto = {
  stepId: string
  status: string
}

export type ProphetPipelineRunStatusDto = {
  versionId: string
  pipelineStatus: string
  currentStepIndex: number
  totalSteps: number
  steps: ProphetPipelineStepStatusDto[]
  error: string | null
  startedAtUtc: string | null
  completedAtUtc: string | null
}

export type ProphetPipelineArtifactDto = {
  artifactType: string
  contentJson: string
  createdByAgent: string | null
  createdAtUtc: string
}

export type ProphetPipelineVersionFileDto = {
  id: string
  fileType: string
  storageObjectPath: string
  originalFileName: string | null
  downloadUrl: string | null
  createdAtUtc: string
}

export type RunPipelineResponseDto = {
  versionId: string
  pipelineStatus: string
  currentStepIndex: number
  totalSteps: number
}

export type RunPipelineRequestBody = {
  interactiveSteps?: boolean
}

async function parsePipelineMutationResult(
  res: Response
): Promise<RunPipelineResponseDto> {
  const json = await readProphetApiResultJson<RunPipelineResponseDto>(res)
  if (!json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Pipeline action failed"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function runProphetPipeline(
  projectId: string,
  versionId: string,
  body: RunPipelineRequestBody | null = null
): Promise<RunPipelineResponseDto> {
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/run`,
    body ?? {}
  )
  return parsePipelineMutationResult(res)
}

export async function continueProphetPipeline(
  projectId: string,
  versionId: string
): Promise<RunPipelineResponseDto> {
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/continue`
  )
  return parsePipelineMutationResult(res)
}

export async function retryProphetPipelineStep(
  projectId: string,
  versionId: string,
  stepIndex: number
): Promise<RunPipelineResponseDto> {
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/steps/retry`,
    { stepIndex }
  )
  return parsePipelineMutationResult(res)
}

export async function rewindProphetPipelineToStep(
  projectId: string,
  versionId: string,
  targetStepIndex: number
): Promise<RunPipelineResponseDto> {
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/steps/rewind`,
    { targetStepIndex }
  )
  return parsePipelineMutationResult(res)
}

export async function listProphetPipelineArtifacts(
  projectId: string,
  versionId: string
): Promise<ProphetPipelineArtifactDto[]> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/artifacts`
  )
  const json = await readProphetApiResultJson<ProphetPipelineArtifactDto[]>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list pipeline artifacts"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

/**
 * Uploads an input document for a specific artifact version (MAF file-agent input).
 */
export async function uploadProphetPipelineVersionInput(
  projectId: string,
  versionId: string,
  file: File
): Promise<ProphetPipelineVersionFileDto> {
  const form = new FormData()
  form.append("file", file)
  const q = new URLSearchParams()
  q.set("versionId", versionId)
  const res = await prophetPost(
    `/v1/prophet/projects/${projectId}/upload?${q.toString()}`,
    form
  )
  const json =
    await readProphetApiResultJson<ProphetPipelineVersionFileDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to upload pipeline input"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function getProphetPipelineRunStatus(
  projectId: string,
  versionId: string
): Promise<ProphetPipelineRunStatusDto> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/status`
  )
  const json = await readProphetApiResultJson<ProphetPipelineRunStatusDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to load pipeline status"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

export async function getProphetPipelineArtifact(
  projectId: string,
  versionId: string,
  artifactType: string
): Promise<ProphetPipelineArtifactDto> {
  const encoded = encodeURIComponent(artifactType)
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/artifacts/${encoded}`
  )
  const json = await readProphetApiResultJson<ProphetPipelineArtifactDto>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to load pipeline artifact"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}

/** Server-side file read (JSON body with text). Avoids CORS on GCS signed GET from the browser. */
export async function getProphetPipelineVersionFileText(
  projectId: string,
  versionId: string,
  fileId: string
): Promise<string> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/files/${encodeURIComponent(fileId)}/content`
  )
  const json = await readProphetApiResultJson<{ text: string }>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to load pipeline file content"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data.text
}

export async function listProphetPipelineVersionFiles(
  projectId: string,
  versionId: string
): Promise<ProphetPipelineVersionFileDto[]> {
  const res = await prophetGet(
    `/v1/prophet/projects/${projectId}/versions/${versionId}/files`
  )
  const json =
    await readProphetApiResultJson<ProphetPipelineVersionFileDto[]>(res)
  if (!res.ok || !json.success || json.data == null) {
    const message =
      json.messages?.join(", ") ||
      `HTTP ${res.status}` ||
      "Failed to list pipeline version files"
    reportRequestError(message)
    throw new Error(message)
  }
  return json.data
}
