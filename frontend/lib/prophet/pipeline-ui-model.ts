/**
 * UI model for Prophet pipeline timeline.
 *
 * Keep this independent from any mock implementation so the UI can be driven by real API data.
 */

export type PipelineStepUiStatus =
  | "completed"
  | "running"
  | "pending"
  | "waiting"
  | "failed"

export type ProphetPipelineStatusDto = {
  versionId: string
  pipelineStatus: string
  currentStepIndex: number
  totalSteps: number
  steps: { stepId: string; status: PipelineStepUiStatus }[]
  error: string | null
  startedAtUtc: string | null
  completedAtUtc: string | null
}

/** Same order as Genesis `MainPipelineStepIds.StepIds`. */
export const PROPHET_PIPELINE_STEP_IDS = [
  "file",
  "insight",
  "market",
  "model",
  "architecture",
  "diagram",
  "poc-web",
  "poc-mobile",
  "doc",
  "packaging",
] as const
