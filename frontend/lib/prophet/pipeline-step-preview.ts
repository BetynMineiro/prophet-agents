import {
  getProphetPipelineArtifact,
  getProphetPipelineVersionFileText,
  listProphetPipelineVersionFiles,
  type ProphetPipelineVersionFileDto,
} from "@/lib/api/prophet/project-pipeline"
import {
  PIPELINE_FILE_TYPE_BY_STEP,
  PIPELINE_JSON_ARTIFACTS_BY_STEP,
} from "@/lib/prophet/pipeline-step-preview-config"

/** Artifact types whose JSON body uses `{ "mermaid": "..." }` for {@link MermaidMarkdown}. */
const MERMAID_JSON_ARTIFACT_TYPES = new Set<string>([
  "class-diagram",
  "flow-diagram",
])

export type PipelineStepPreviewResult =
  | { mode: "markdown"; markdown: string }
  | { mode: "iframe"; url: string; fileName: string }

function formatJsonArtifactBlock(
  artifactType: string,
  contentJson: string
): string {
  if (MERMAID_JSON_ARTIFACT_TYPES.has(artifactType)) {
    try {
      const parsed = JSON.parse(contentJson.trim()) as unknown
      if (
        parsed !== null &&
        typeof parsed === "object" &&
        !Array.isArray(parsed) &&
        "mermaid" in parsed &&
        typeof (parsed as { mermaid?: unknown }).mermaid === "string"
      ) {
        const chart = (parsed as { mermaid: string }).mermaid.trim()
        if (chart.length > 0) {
          return `\`\`\`mermaid\n${chart}\n\`\`\``
        }
      }
    } catch {
      /* fall through to JSON fence */
    }
  }
  const body = prettyJsonForMarkdown(contentJson)
  return `\`\`\`json\n${body}\n\`\`\``
}

export function prettyJsonForMarkdown(raw: string): string {
  const trimmed = raw.trim()
  try {
    return JSON.stringify(JSON.parse(trimmed), null, 2)
  } catch {
    return trimmed
  }
}

function pickFileForStep(
  files: readonly ProphetPipelineVersionFileDto[],
  stepIndex: number
): ProphetPipelineVersionFileDto | null {
  const fileType = PIPELINE_FILE_TYPE_BY_STEP[stepIndex]
  if (!fileType) return null
  return (
    files.find(
      (f) =>
        f.fileType === fileType && f.downloadUrl && f.downloadUrl.length > 0
    ) ?? null
  )
}

/**
 * Loads pipeline step preview. POC Web/Mobile use `iframe` signed URLs. Markdown file steps use
 * Prophet `GET .../files/{id}/content` (server reads GCS) — browser `fetch()` to signed GCS URLs fails CORS.
 */
export async function loadPipelineStepPreview(
  projectId: string,
  versionId: string,
  stepIndex: number
): Promise<PipelineStepPreviewResult> {
  const jsonSpec = PIPELINE_JSON_ARTIFACTS_BY_STEP[stepIndex]
  if (jsonSpec != null) {
    const types = typeof jsonSpec === "string" ? [jsonSpec] : [...jsonSpec]
    const parts: string[] = []
    for (const t of types) {
      const art = await getProphetPipelineArtifact(projectId, versionId, t)
      const block = formatJsonArtifactBlock(t, art.contentJson)
      parts.push(`## \`${t}\`\n\n${block}\n`)
    }
    return { mode: "markdown", markdown: parts.join("\n") }
  }

  if (PIPELINE_FILE_TYPE_BY_STEP[stepIndex] != null) {
    const files = await listProphetPipelineVersionFiles(projectId, versionId)
    const file = pickFileForStep(files, stepIndex)
    if (!file?.downloadUrl) {
      throw new Error("No file for this step")
    }

    if (stepIndex === 6 || stepIndex === 7) {
      return {
        mode: "iframe",
        url: file.downloadUrl,
        fileName: file.originalFileName ?? `step-${stepIndex}.html`,
      }
    }

    const text = await getProphetPipelineVersionFileText(
      projectId,
      versionId,
      file.id
    )
    return { mode: "markdown", markdown: text }
  }

  throw new Error("No preview for this step")
}
