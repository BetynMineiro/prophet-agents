/** 0-based step index → primary JSON artifact type(s) produced when the step succeeds. */
export const PIPELINE_JSON_ARTIFACTS_BY_STEP: Record<
  number,
  string | readonly string[]
> = {
  0: "chunks",
  1: "insights",
  2: "market-analysis",
  3: "domain-model",
  4: "architecture",
  5: ["class-diagram", "flow-diagram"],
  9: "packaging-manifest",
}

/** File step: pipeline version file type (matches Genesis `ArtifactFileTypeNames`). */
export const PIPELINE_FILE_TYPE_BY_STEP: Record<number, string> = {
  6: "poc-web",
  7: "poc-mobile",
  8: "documentation",
}
