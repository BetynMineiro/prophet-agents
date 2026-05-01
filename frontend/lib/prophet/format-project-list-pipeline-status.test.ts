import { describe, expect, it } from "vitest"
import { formatProphetProjectListPipelineStatus } from "./format-project-list-pipeline-status"

describe("formatProphetProjectListPipelineStatus", () => {
  it("returns '—' when status is null", () => {
    expect(formatProphetProjectListPipelineStatus(null)).toBe("—")
  })

  it("returns '—' when status is undefined", () => {
    expect(formatProphetProjectListPipelineStatus(undefined)).toBe("—")
  })

  it("returns '—' when status is empty string", () => {
    expect(formatProphetProjectListPipelineStatus("")).toBe("—")
  })

  it("returns '—' when status is whitespace only", () => {
    expect(formatProphetProjectListPipelineStatus("   ")).toBe("—")
  })

  it("returns 'Idle' for 'Idle' (case-insensitive)", () => {
    expect(formatProphetProjectListPipelineStatus("Idle")).toBe("Idle")
  })

  it("returns 'Idle' for 'IDLE' (uppercase)", () => {
    expect(formatProphetProjectListPipelineStatus("IDLE")).toBe("Idle")
  })

  it("returns 'Running' for 'running'", () => {
    expect(formatProphetProjectListPipelineStatus("running")).toBe("Running")
  })

  it("returns 'Completed' for 'Completed'", () => {
    expect(formatProphetProjectListPipelineStatus("Completed")).toBe("Completed")
  })

  it("returns 'Failed' for 'failed'", () => {
    expect(formatProphetProjectListPipelineStatus("failed")).toBe("Failed")
  })

  it("returns 'Paused' for 'paused'", () => {
    expect(formatProphetProjectListPipelineStatus("paused")).toBe("Paused")
  })

  it("returns trimmed status as-is for unknown status", () => {
    expect(formatProphetProjectListPipelineStatus("  unknown-state  ")).toBe(
      "unknown-state"
    )
  })
})
