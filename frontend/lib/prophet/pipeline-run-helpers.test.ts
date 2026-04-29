import { describe, expect, it } from "vitest"
import {
  prophetPipelineStatusAllowsContinue,
  prophetPipelineStatusAllowsFullRun,
  prophetPipelineStatusAllowsInteractiveActions,
} from "./pipeline-run-helpers"

describe("pipeline-run-helpers", () => {
  it("detects paused for interactive actions and continue", () => {
    expect(prophetPipelineStatusAllowsInteractiveActions("Paused")).toBe(true)
    expect(prophetPipelineStatusAllowsInteractiveActions("Failed")).toBe(true)
    expect(prophetPipelineStatusAllowsInteractiveActions("Completed")).toBe(
      true
    )
    expect(prophetPipelineStatusAllowsContinue("Paused")).toBe(true)
    expect(prophetPipelineStatusAllowsInteractiveActions("Idle")).toBe(false)
  })

  it("allows full run for idle, failed, and completed (restart)", () => {
    expect(prophetPipelineStatusAllowsFullRun("Idle")).toBe(true)
    expect(prophetPipelineStatusAllowsFullRun("Failed")).toBe(true)
    expect(prophetPipelineStatusAllowsFullRun("Completed")).toBe(true)
    expect(prophetPipelineStatusAllowsFullRun("Running")).toBe(false)
  })

  it("trims status strings (whitespace-insensitive)", () => {
    expect(prophetPipelineStatusAllowsContinue("  paused  ")).toBe(true)
    expect(prophetPipelineStatusAllowsFullRun("  idle  ")).toBe(true)
    expect(prophetPipelineStatusAllowsInteractiveActions(" completed ")).toBe(
      true
    )
  })

  it("rejects continue when not paused", () => {
    expect(prophetPipelineStatusAllowsContinue("Running")).toBe(false)
    expect(prophetPipelineStatusAllowsContinue("Idle")).toBe(false)
    expect(prophetPipelineStatusAllowsContinue("Completed")).toBe(false)
  })
})
