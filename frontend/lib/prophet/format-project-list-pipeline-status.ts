/**
 * Labels the pipeline run state for the Prophet projects table (Genesis API strings: Idle, Running, …).
 */
export function formatProphetProjectListPipelineStatus(
  status: string | null | undefined,
  t: (key: string) => string
): string {
  if (status == null || status.trim() === "") {
    return t("prophetProjectListPipelineNone")
  }
  const low = status.trim().toLowerCase()
  switch (low) {
    case "idle":
      return t("prophetProjectListPipelineStatus_idle")
    case "running":
      return t("prophetProjectListPipelineStatus_running")
    case "completed":
      return t("prophetProjectListPipelineStatus_completed")
    case "failed":
      return t("prophetProjectListPipelineStatus_failed")
    case "paused":
      return t("prophetProjectListPipelineStatus_paused")
    default:
      return status.trim()
  }
}
