import type { CSSProperties } from "react"
import type {
  PipelineStepUiStatus,
  ProphetPipelineStatusDto,
} from "@/lib/prophet/pipeline-ui-model"
import {
  PIPELINE_CONNECTOR_PX,
  PIPELINE_MAX_COLUMNS,
  PIPELINE_MIN_CARD_PX,
  PIPELINE_MIN_COLUMNS,
} from "@/components/features/prophet/pipeline-timeline/pipeline-timeline-constants"

export function computePipelineColumns(containerWidthPx: number): number {
  if (!Number.isFinite(containerWidthPx) || containerWidthPx <= 0) {
    return 5
  }
  for (let n = PIPELINE_MAX_COLUMNS; n >= PIPELINE_MIN_COLUMNS; n--) {
    const total = n * PIPELINE_MIN_CARD_PX + (n - 1) * PIPELINE_CONNECTOR_PX
    if (total <= containerWidthPx) {
      return n
    }
  }
  return PIPELINE_MIN_COLUMNS
}

/** Same width every card would have in a full row (avoids last row stretching). */
export function pipelineCardFlexStyle(columns: number): CSSProperties {
  const c = Math.max(
    PIPELINE_MIN_COLUMNS,
    Math.min(PIPELINE_MAX_COLUMNS, columns)
  )
  if (c <= 1) {
    return { flex: "0 0 100%" }
  }
  const basis = `calc((100% - ${(c - 1) * PIPELINE_CONNECTOR_PX}px) / ${c})`
  return { flex: `0 0 ${basis}` }
}

export function chunkStepsWithStartIndex(
  steps: ProphetPipelineStatusDto["steps"],
  columns: number
): { steps: ProphetPipelineStatusDto["steps"]; startIndex: number }[] {
  const c = Math.max(
    PIPELINE_MIN_COLUMNS,
    Math.min(PIPELINE_MAX_COLUMNS, columns)
  )
  const out: {
    steps: ProphetPipelineStatusDto["steps"]
    startIndex: number
  }[] = []
  for (let i = 0; i < steps.length; i += c) {
    out.push({
      steps: steps.slice(i, i + c),
      startIndex: i,
    })
  }
  return out
}

export function statusBadgeVariant(
  status: string
): "default" | "secondary" | "destructive" | "outline" {
  const s = status.toLowerCase()
  if (s === "completed") return "default"
  if (s === "failed") return "destructive"
  if (s === "running") return "secondary"
  if (s === "paused") return "outline"
  return "secondary"
}

export function stepBoxTone(status: PipelineStepUiStatus): string {
  switch (status) {
    case "completed":
      return "border-emerald-500/35 bg-emerald-500/[0.06] dark:border-emerald-500/40"
    case "running":
      return "border-primary/50 bg-primary/[0.06] ring-1 ring-primary/20"
    case "failed":
      return "border-destructive/45 bg-destructive/[0.06]"
    case "waiting":
      return "border-amber-500/45 bg-amber-500/[0.08]"
    case "pending":
    default:
      return "border-border bg-muted/40"
  }
}
