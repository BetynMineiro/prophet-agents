/**
 * Labels the pipeline run state for the Prophet projects table (Genesis API strings: Idle, Running, …).
 */
export function formatProphetProjectListPipelineStatus(
  status: string | null | undefined
): string {
  if (status == null || status.trim() === "") {
    return "—"
  }
  const low = status.trim().toLowerCase()
  switch (low) {
    case "idle":
      return "Idle"
    case "running":
      return "Running"
    case "completed":
      return "Completed"
    case "failed":
      return "Failed"
    case "paused":
      return "Paused"
    default:
      return status.trim()
  }
}
