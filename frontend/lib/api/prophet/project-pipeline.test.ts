import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  continueProphetPipeline,
  getProphetPipelineArtifact,
  getProphetPipelineRunStatus,
  getProphetPipelineVersionFileText,
  listProphetPipelineArtifacts,
  listProphetPipelineVersionFiles,
  retryProphetPipelineStep,
  rewindProphetPipelineToStep,
  runProphetPipeline,
  uploadProphetPipelineVersionInput,
} from "./project-pipeline"
import { prophetTestJsonResponse } from "@/lib/api/prophet/test-json-response"
import { prophetGet, prophetPost } from "@/lib/api/client"

vi.mock("@/lib/api/client", () => ({
  prophetGet: vi.fn(),
  prophetPost: vi.fn(),
}))

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: vi.fn(),
}))

describe("project-pipeline", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)
  const projectId = "019d0000-0000-7000-8000-000000000001"
  const versionId = "019d0000-0000-7000-8000-000000000002"

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("getProphetPipelineRunStatus returns data on success", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Running",
      currentStepIndex: 2,
      totalSteps: 10,
      steps: [{ stepId: "file", status: "completed" }],
      error: null,
      startedAtUtc: "2026-01-01T00:00:00Z",
      completedAtUtc: null,
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )

    const result = await getProphetPipelineRunStatus(projectId, versionId)
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/status`
    )
    expect(result).toEqual(dto)
  })

  it("getProphetPipelineArtifact encodes artifact type in path", async () => {
    const art = {
      artifactType: "market-analysis",
      contentJson: "{}",
      createdByAgent: "agent",
      createdAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: art })
    )

    await getProphetPipelineArtifact(projectId, versionId, "market-analysis")
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/artifacts/market-analysis`
    )
  })

  it("listProphetPipelineVersionFiles returns data on success", async () => {
    const files = [
      {
        id: "019d0000-0000-7000-8000-000000000003",
        fileType: "documentation",
        storageObjectPath: "x",
        originalFileName: "doc.md",
        downloadUrl: "https://example.com/signed",
        createdAtUtc: "2026-01-01T00:00:00Z",
      },
    ]
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: files })
    )

    const result = await listProphetPipelineVersionFiles(projectId, versionId)
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/files`
    )
    expect(result).toEqual(files)
  })

  it("getProphetPipelineRunStatus throws when success false", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse(
        { success: false, messages: ["nope"] },
        { ok: false }
      )
    )
    await expect(
      getProphetPipelineRunStatus(projectId, versionId)
    ).rejects.toThrow()
  })

  it("runProphetPipeline posts interactiveSteps", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Running",
      currentStepIndex: 0,
      totalSteps: 10,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    const result = await runProphetPipeline(projectId, versionId, {
      interactiveSteps: true,
    })
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/run`,
      { interactiveSteps: true }
    )
    expect(result).toEqual(dto)
  })

  it("continueProphetPipeline posts to pipeline/continue", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Paused",
      currentStepIndex: 3,
      totalSteps: 10,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    const result = await continueProphetPipeline(projectId, versionId)
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/continue`
    )
    expect(result).toEqual(dto)
  })

  it("runProphetPipeline sends empty object when body is null", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Running",
      currentStepIndex: 0,
      totalSteps: 10,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    await runProphetPipeline(projectId, versionId, null)
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/run`,
      {}
    )
  })

  it("retryProphetPipelineStep posts stepIndex", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Running",
      currentStepIndex: 2,
      totalSteps: 10,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    const result = await retryProphetPipelineStep(projectId, versionId, 2)
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/steps/retry`,
      { stepIndex: 2 }
    )
    expect(result).toEqual(dto)
  })

  it("rewindProphetPipelineToStep posts targetStepIndex", async () => {
    const dto = {
      versionId,
      pipelineStatus: "Idle",
      currentStepIndex: 1,
      totalSteps: 10,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    const result = await rewindProphetPipelineToStep(projectId, versionId, 3)
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/pipeline/steps/rewind`,
      { targetStepIndex: 3 }
    )
    expect(result).toEqual(dto)
  })

  it("uploadProphetPipelineVersionInput posts FormData with versionId query", async () => {
    const file = new File(["x"], "in.txt", { type: "text/plain" })
    const dto = {
      id: "019d0000-0000-7000-8000-000000000099",
      fileType: "input",
      storageObjectPath: "p",
      originalFileName: "in.txt",
      downloadUrl: null,
      createdAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )
    const result = await uploadProphetPipelineVersionInput(
      projectId,
      versionId,
      file
    )
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/upload?versionId=${encodeURIComponent(versionId)}`,
      expect.any(FormData)
    )
    expect(result).toEqual(dto)
  })

  it("continueProphetPipeline throws when envelope success is false", async () => {
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse(
        { success: false, messages: ["bad"] },
        { ok: false, status: 400 }
      )
    )
    await expect(
      continueProphetPipeline(projectId, versionId)
    ).rejects.toThrow()
  })

  it("listProphetPipelineArtifacts returns list", async () => {
    const list = [
      {
        artifactType: "chunks",
        contentJson: "{}",
        createdByAgent: "file-agent",
        createdAtUtc: "2026-01-01T00:00:00Z",
      },
    ]
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: list })
    )
    const result = await listProphetPipelineArtifacts(projectId, versionId)
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/artifacts`
    )
    expect(result).toEqual(list)
  })

  it("getProphetPipelineVersionFileText returns text from Result envelope", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { text: "# Doc\n" },
      })
    )
    const fileId = "019d0000-0000-7000-8000-0000000000aa"
    const result = await getProphetPipelineVersionFileText(
      projectId,
      versionId,
      fileId
    )
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions/${versionId}/files/${encodeURIComponent(fileId)}/content`
    )
    expect(result).toBe("# Doc\n")
  })
})
