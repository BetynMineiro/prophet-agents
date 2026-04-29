/**
 * When Interactive (auto-continue) is on, the client POSTs Continue after each Paused state.
 * Users can mark step indices to pause for manual review (Continue / Retry / Rewind) instead.
 *
 * `currentStepIndex` while Paused is the next step to run (see Genesis `PipelineExecutionStatusHelper`).
 */
export function shouldScheduleAutoContinue(params: {
  runInteractive: boolean
  pipelineStatusLower: string
  currentStepIndex: number
  pauseAtStepIndices: ReadonlySet<number>
}): boolean {
  if (!params.runInteractive) return false
  if (params.pipelineStatusLower !== "paused") return false
  if (params.pauseAtStepIndices.has(params.currentStepIndex)) return false
  return true
}
