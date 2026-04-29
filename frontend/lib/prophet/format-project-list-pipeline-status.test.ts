import { describe, expect, it } from "vitest"
import { formatProphetProjectListPipelineStatus } from "./format-project-list-pipeline-status"

const t = (key: string) => key

describe("formatProphetProjectListPipelineStatus", () => {
  it("returns 'none' key when status is null", () => {
    expect(formatProphetProjectListPipelineStatus(null, t)).toBe(
      "prophetProjectListPipelineNone"
    )
  })

  it("returns 'none' key when status is undefined", () => {
    expect(formatProphetProjectListPipelineStatus(undefined, t)).toBe(
      "prophetProjectListPipelineNone"
    )
  })

  it("returns 'none' key when status is empty string", () => {
    expect(formatProphetProjectListPipelineStatus("", t)).toBe(
      "prophetProjectListPipelineNone"
    )
  })

  it("returns 'none' key when status is whitespace only", () => {
    expect(formatProphetProjectListPipelineStatus("   ", t)).toBe(
      "prophetProjectListPipelineNone"
    )
  })

  it("returns idle key for 'Idle' (case-insensitive)", () => {
    expect(formatProphetProjectListPipelineStatus("Idle", t)).toBe(
      "prophetProjectListPipelineStatus_idle"
    )
  })

  it("returns idle key for 'IDLE' (uppercase)", () => {
    expect(formatProphetProjectListPipelineStatus("IDLE", t)).toBe(
      "prophetProjectListPipelineStatus_idle"
    )
  })

  it("returns running key for 'running'", () => {
    expect(formatProphetProjectListPipelineStatus("running", t)).toBe(
      "prophetProjectListPipelineStatus_running"
    )
  })

  it("returns completed key for 'Completed'", () => {
    expect(formatProphetProjectListPipelineStatus("Completed", t)).toBe(
      "prophetProjectListPipelineStatus_completed"
    )
  })

  it("returns failed key for 'failed'", () => {
    expect(formatProphetProjectListPipelineStatus("failed", t)).toBe(
      "prophetProjectListPipelineStatus_failed"
    )
  })

  it("returns paused key for 'paused'", () => {
    expect(formatProphetProjectListPipelineStatus("paused", t)).toBe(
      "prophetProjectListPipelineStatus_paused"
    )
  })

  it("returns trimmed status as-is for unknown status", () => {
    expect(formatProphetProjectListPipelineStatus("  unknown-state  ", t)).toBe(
      "unknown-state"
    )
  })

  it("calls translation function with correct key", () => {
    const calls: string[] = []
    const tSpy = (key: string) => {
      calls.push(key)
      return key
    }
    formatProphetProjectListPipelineStatus("running", tSpy)
    expect(calls).toEqual(["prophetProjectListPipelineStatus_running"])
  })
})
