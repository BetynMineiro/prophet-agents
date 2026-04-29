import { describe, expect, it } from "vitest"
import { shouldScheduleAutoContinue } from "@/lib/prophet/pipeline-auto-continue-breakpoints"

describe("shouldScheduleAutoContinue", () => {
  it("returns false when interactive is off", () => {
    expect(
      shouldScheduleAutoContinue({
        runInteractive: false,
        pipelineStatusLower: "paused",
        currentStepIndex: 2,
        pauseAtStepIndices: new Set([2]),
      })
    ).toBe(false)
  })

  it("returns false when status is not paused", () => {
    expect(
      shouldScheduleAutoContinue({
        runInteractive: true,
        pipelineStatusLower: "running",
        currentStepIndex: 2,
        pauseAtStepIndices: new Set(),
      })
    ).toBe(false)
  })

  it("returns false when current step is a breakpoint", () => {
    expect(
      shouldScheduleAutoContinue({
        runInteractive: true,
        pipelineStatusLower: "paused",
        currentStepIndex: 5,
        pauseAtStepIndices: new Set([5]),
      })
    ).toBe(false)
  })

  it("returns true when interactive, paused, and step is not a breakpoint", () => {
    expect(
      shouldScheduleAutoContinue({
        runInteractive: true,
        pipelineStatusLower: "paused",
        currentStepIndex: 3,
        pauseAtStepIndices: new Set([1, 5]),
      })
    ).toBe(true)
  })
})
