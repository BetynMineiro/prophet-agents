import { describe, expect, it } from "vitest"
import {
  PIPELINE_JSON_ARTIFACTS_BY_STEP,
  PIPELINE_FILE_TYPE_BY_STEP,
} from "./pipeline-step-preview-config"

describe("PIPELINE_JSON_ARTIFACTS_BY_STEP", () => {
  it("step 0 produces chunks", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[0]).toBe("chunks")
  })

  it("step 1 produces insights", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[1]).toBe("insights")
  })

  it("step 2 produces market-analysis", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[2]).toBe("market-analysis")
  })

  it("step 3 produces domain-model", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[3]).toBe("domain-model")
  })

  it("step 4 produces architecture", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[4]).toBe("architecture")
  })

  it("step 5 produces both diagram types as array", () => {
    const step5 = PIPELINE_JSON_ARTIFACTS_BY_STEP[5]
    expect(Array.isArray(step5)).toBe(true)
    expect(step5).toContain("class-diagram")
    expect(step5).toContain("flow-diagram")
  })

  it("step 9 produces packaging-manifest", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[9]).toBe("packaging-manifest")
  })

  it("file steps (6, 7, 8) are not in the JSON artifacts map", () => {
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[6]).toBeUndefined()
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[7]).toBeUndefined()
    expect(PIPELINE_JSON_ARTIFACTS_BY_STEP[8]).toBeUndefined()
  })
})

describe("PIPELINE_FILE_TYPE_BY_STEP", () => {
  it("step 6 is poc-web", () => {
    expect(PIPELINE_FILE_TYPE_BY_STEP[6]).toBe("poc-web")
  })

  it("step 7 is poc-mobile", () => {
    expect(PIPELINE_FILE_TYPE_BY_STEP[7]).toBe("poc-mobile")
  })

  it("step 8 is documentation", () => {
    expect(PIPELINE_FILE_TYPE_BY_STEP[8]).toBe("documentation")
  })

  it("JSON artifact steps are not in the file type map", () => {
    expect(PIPELINE_FILE_TYPE_BY_STEP[0]).toBeUndefined()
    expect(PIPELINE_FILE_TYPE_BY_STEP[5]).toBeUndefined()
    expect(PIPELINE_FILE_TYPE_BY_STEP[9]).toBeUndefined()
  })
})
