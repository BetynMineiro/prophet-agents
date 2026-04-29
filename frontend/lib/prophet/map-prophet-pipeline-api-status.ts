import type { ProphetPipelineRunStatusDto } from "@/lib/api/prophet/project-pipeline"
import type {
  PipelineStepUiStatus,
  ProphetPipelineStatusDto,
} from "@/lib/prophet/pipeline-ui-model"

function apiStepStatusToUi(s: string): PipelineStepUiStatus {
  const x = s.toLowerCase()
  if (
    x === "completed" ||
    x === "running" ||
    x === "pending" ||
    x === "waiting" ||
    x === "failed"
  ) {
    return x
  }
  return "pending"
}

/** Maps Genesis `GET .../status` DTO to the UI model used by the pipeline timeline. */
export function mapProphetPipelineRunStatusToUi(
  api: ProphetPipelineRunStatusDto
): ProphetPipelineStatusDto {
  return {
    versionId: api.versionId,
    pipelineStatus: api.pipelineStatus,
    currentStepIndex: api.currentStepIndex,
    totalSteps: api.totalSteps,
    error: api.error,
    startedAtUtc: api.startedAtUtc,
    completedAtUtc: api.completedAtUtc,
    steps: api.steps.map((st) => ({
      stepId: st.stepId,
      status: apiStepStatusToUi(st.status),
    })),
  }
}
