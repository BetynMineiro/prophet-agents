import { describe, expect, it, vi, beforeEach } from "vitest"

vi.mock("@/lib/api/prophet/project-pipeline", () => ({
  getProphetPipelineArtifact: vi.fn(),
  getProphetPipelineVersionFileText: vi.fn(),
  listProphetPipelineVersionFiles: vi.fn(),
}))

import {
  getProphetPipelineArtifact,
  getProphetPipelineVersionFileText,
  listProphetPipelineVersionFiles,
} from "@/lib/api/prophet/project-pipeline"
import {
  loadPipelineStepPreview,
  prettyJsonForMarkdown,
} from "./pipeline-step-preview"

describe("pipeline-step-preview", () => {
  const getArtifact = vi.mocked(getProphetPipelineArtifact)
  const getFileText = vi.mocked(getProphetPipelineVersionFileText)
  const listFiles = vi.mocked(listProphetPipelineVersionFiles)
  const projectId = "019d0000-0000-7000-8000-000000000001"
  const versionId = "019d0000-0000-7000-8000-000000000002"

  beforeEach(() => {
    vi.clearAllMocks()
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        text: () => Promise.resolve("<section>html</section>"),
      })
    )
  })

  it("prettyJsonForMarkdown formats valid JSON and returns raw on parse failure", () => {
    expect(prettyJsonForMarkdown('  {"a":1}  ')).toBe(
      JSON.stringify({ a: 1 }, null, 2)
    )
    expect(prettyJsonForMarkdown(" not json ")).toBe("not json")
  })

  it("loadPipelineStepPreview builds fenced JSON for a single artifact step", async () => {
    getArtifact.mockResolvedValueOnce({
      artifactType: "chunks",
      contentJson: '{"x":1}',
      createdByAgent: null,
      createdAtUtc: "2026-01-01T00:00:00Z",
    })

    const r = await loadPipelineStepPreview(projectId, versionId, 0)
    expect(r.mode).toBe("markdown")
    if (r.mode !== "markdown") throw new Error("expected markdown")
    expect(getArtifact).toHaveBeenCalledWith(projectId, versionId, "chunks")
    expect(r.markdown).toContain("## `chunks`")
    expect(r.markdown).toContain('"x": 1')
  })

  it("loadPipelineStepPreview renders mermaid fences for diagram artifacts on step 5", async () => {
    getArtifact
      .mockResolvedValueOnce({
        artifactType: "class-diagram",
        contentJson: JSON.stringify({
          mermaid: "classDiagram\nclass Tenant\nclass License",
        }),
        createdByAgent: null,
        createdAtUtc: "2026-01-01T00:00:00Z",
      })
      .mockResolvedValueOnce({
        artifactType: "flow-diagram",
        contentJson: JSON.stringify({
          mermaid: "flowchart TD\nA-->B",
        }),
        createdByAgent: null,
        createdAtUtc: "2026-01-01T00:00:00Z",
      })

    const r = await loadPipelineStepPreview(projectId, versionId, 5)
    expect(r.mode).toBe("markdown")
    if (r.mode !== "markdown") throw new Error("expected markdown")
    expect(getArtifact).toHaveBeenCalledTimes(2)
    expect(r.markdown).toContain("## `class-diagram`")
    expect(r.markdown).toContain("## `flow-diagram`")
    expect(r.markdown).toContain("```mermaid")
    expect(r.markdown).toContain("classDiagram")
    expect(r.markdown).toContain("flowchart TD")
    expect(r.markdown).not.toContain("```json")
  })

  it("loadPipelineStepPreview falls back to JSON fence for diagram artifacts without mermaid", async () => {
    getArtifact
      .mockResolvedValueOnce({
        artifactType: "class-diagram",
        contentJson: "{}",
        createdByAgent: null,
        createdAtUtc: "2026-01-01T00:00:00Z",
      })
      .mockResolvedValueOnce({
        artifactType: "flow-diagram",
        contentJson: "[]",
        createdByAgent: null,
        createdAtUtc: "2026-01-01T00:00:00Z",
      })

    const r = await loadPipelineStepPreview(projectId, versionId, 5)
    expect(r.mode).toBe("markdown")
    if (r.mode !== "markdown") throw new Error("expected markdown")
    expect(r.markdown).toContain("```json")
  })

  it("loadPipelineStepPreview returns iframe URL for poc-web without fetch (avoids GCS CORS)", async () => {
    listFiles.mockResolvedValueOnce([
      {
        id: "f1",
        fileType: "poc-web",
        storageObjectPath: "p",
        originalFileName: "app.html",
        downloadUrl: "https://signed.example/get",
        createdAtUtc: "2026-01-01T00:00:00Z",
      },
    ])

    const r = await loadPipelineStepPreview(projectId, versionId, 6)
    expect(r.mode).toBe("iframe")
    if (r.mode !== "iframe") throw new Error("expected iframe")
    expect(listFiles).toHaveBeenCalledWith(projectId, versionId)
    expect(r.url).toBe("https://signed.example/get")
    expect(r.fileName).toBe("app.html")
    expect(vi.mocked(globalThis.fetch)).not.toHaveBeenCalled()
  })

  it("loadPipelineStepPreview returns raw markdown text for documentation step via Prophet content API", async () => {
    listFiles.mockResolvedValueOnce([
      {
        id: "f1",
        fileType: "documentation",
        storageObjectPath: "p",
        originalFileName: "doc.md",
        downloadUrl: "https://signed.example/doc",
        createdAtUtc: "2026-01-01T00:00:00Z",
      },
    ])
    getFileText.mockResolvedValueOnce("# Title\n")

    const r = await loadPipelineStepPreview(projectId, versionId, 8)
    expect(r.mode).toBe("markdown")
    if (r.mode !== "markdown") throw new Error("expected markdown")
    expect(getFileText).toHaveBeenCalledWith(projectId, versionId, "f1")
    expect(r.markdown).toBe("# Title\n")
  })

  it("loadPipelineStepPreview throws when no file for file step", async () => {
    listFiles.mockResolvedValueOnce([])
    await expect(
      loadPipelineStepPreview(projectId, versionId, 6)
    ).rejects.toThrow("No file for this step")
  })

  it("loadPipelineStepPreview throws when step has no preview mapping", async () => {
    await expect(
      loadPipelineStepPreview(projectId, versionId, 99)
    ).rejects.toThrow("No preview for this step")
  })
})
