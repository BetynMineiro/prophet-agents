import { describe, expect, it } from "vitest"
import {
  chunkStepsWithStartIndex,
  computePipelineColumns,
  pipelineCardFlexStyle,
  statusBadgeVariant,
  stepBoxTone,
} from "./pipeline-timeline-layout"
import type { ProphetPipelineStatusDto } from "@/lib/prophet/pipeline-ui-model"

describe("pipeline-timeline-layout", () => {
  it("computePipelineColumns returns default when width invalid", () => {
    expect(computePipelineColumns(Number.NaN)).toBe(5)
    expect(computePipelineColumns(0)).toBe(5)
    expect(computePipelineColumns(-1)).toBe(5)
  })

  it("computePipelineColumns picks largest column count that fits", () => {
    expect(computePipelineColumns(2000)).toBe(10)
    expect(computePipelineColumns(400)).toBe(2)
  })

  it("pipelineCardFlexStyle clamps to min columns and uses calc for multi-column", () => {
    expect(pipelineCardFlexStyle(1)).toEqual({
      flex: "0 0 calc((100% - 20px) / 2)",
    })
    expect(pipelineCardFlexStyle(3)).toMatchObject({
      flex: expect.stringMatching(/^0 0 calc/),
    })
  })

  it("chunkStepsWithStartIndex groups by column count", () => {
    const steps: ProphetPipelineStatusDto["steps"] = [
      { stepId: "a", status: "completed" },
      { stepId: "b", status: "pending" },
      { stepId: "c", status: "pending" },
    ]
    const chunks = chunkStepsWithStartIndex(steps, 2)
    expect(chunks).toHaveLength(2)
    expect(chunks[0]).toEqual({
      steps: steps.slice(0, 2),
      startIndex: 0,
    })
    expect(chunks[1]).toEqual({
      steps: steps.slice(2, 3),
      startIndex: 2,
    })
  })

  it("statusBadgeVariant maps known statuses", () => {
    expect(statusBadgeVariant("Completed")).toBe("default")
    expect(statusBadgeVariant("FAILED")).toBe("destructive")
    expect(statusBadgeVariant("Running")).toBe("secondary")
    expect(statusBadgeVariant("Paused")).toBe("outline")
    expect(statusBadgeVariant("unknown")).toBe("secondary")
  })

  it("stepBoxTone returns classes per ui status", () => {
    expect(stepBoxTone("completed")).toContain("emerald")
    expect(stepBoxTone("running")).toContain("primary")
    expect(stepBoxTone("failed")).toContain("destructive")
    expect(stepBoxTone("waiting")).toContain("amber")
    expect(stepBoxTone("pending")).toContain("muted")
  })
})
