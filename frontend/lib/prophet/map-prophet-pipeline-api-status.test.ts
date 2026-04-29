import { describe, expect, it } from "vitest"
import { mapProphetPipelineRunStatusToUi } from "@/lib/prophet/map-prophet-pipeline-api-status"
import type { ProphetPipelineRunStatusDto } from "@/lib/api/prophet/project-pipeline"

describe("mapProphetPipelineRunStatusToUi", () => {
  it("maps known step statuses to UI literals", () => {
    const api: ProphetPipelineRunStatusDto = {
      versionId: "v1",
      pipelineStatus: "Running",
      currentStepIndex: 1,
      totalSteps: 10,
      steps: [
        { stepId: "file", status: "completed" },
        { stepId: "insight", status: "COMPLETED" },
        { stepId: "market", status: "unknown" },
      ],
      error: null,
      startedAtUtc: null,
      completedAtUtc: null,
    }
    const ui = mapProphetPipelineRunStatusToUi(api)
    expect(ui.steps[0]?.status).toBe("completed")
    expect(ui.steps[1]?.status).toBe("completed")
    expect(ui.steps[2]?.status).toBe("pending")
  })

  it("copies version and pipeline fields", () => {
    const api: ProphetPipelineRunStatusDto = {
      versionId: "vid",
      pipelineStatus: "Paused",
      currentStepIndex: 3,
      totalSteps: 10,
      steps: [],
      error: "x",
      startedAtUtc: "2026-01-01T00:00:00Z",
      completedAtUtc: null,
    }
    const ui = mapProphetPipelineRunStatusToUi(api)
    expect(ui.versionId).toBe("vid")
    expect(ui.pipelineStatus).toBe("Paused")
    expect(ui.currentStepIndex).toBe(3)
    expect(ui.error).toBe("x")
    expect(ui.startedAtUtc).toBe("2026-01-01T00:00:00Z")
  })

  it("maps running, waiting, and failed step statuses case-insensitively", () => {
    const api: ProphetPipelineRunStatusDto = {
      versionId: "v",
      pipelineStatus: "Paused",
      currentStepIndex: 2,
      totalSteps: 5,
      steps: [
        { stepId: "file", status: "RUNNING" },
        { stepId: "insight", status: "Waiting" },
        { stepId: "market", status: "FAILED" },
      ],
      error: null,
      startedAtUtc: null,
      completedAtUtc: null,
    }
    const ui = mapProphetPipelineRunStatusToUi(api)
    expect(ui.steps[0]?.status).toBe("running")
    expect(ui.steps[1]?.status).toBe("waiting")
    expect(ui.steps[2]?.status).toBe("failed")
  })
})
