/** Genesis `PipelineRunStatus` string (e.g. Idle, Paused). */
export function prophetPipelineStatusAllowsInteractiveActions(
  status: string
): boolean {
  const s = status.trim().toLowerCase()
  return s === "paused" || s === "failed" || s === "completed"
}

/** Full Run restarts from step 0 when Idle, Failed, or Completed (clears prior outputs). */
export function prophetPipelineStatusAllowsFullRun(status: string): boolean {
  const s = status.trim().toLowerCase()
  return s === "idle" || s === "failed" || s === "completed"
}

export function prophetPipelineStatusAllowsContinue(status: string): boolean {
  return status.trim().toLowerCase() === "paused"
}
