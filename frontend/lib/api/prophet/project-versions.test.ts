import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  createProphetArtifactVersion,
  listProphetProjectArtifactVersions,
} from "./project-versions"
import { prophetTestJsonResponse } from "@/lib/api/prophet/test-json-response"
import { prophetGet, prophetPost } from "@/lib/api/client"

vi.mock("@/lib/api/client", () => ({
  prophetGet: vi.fn(),
  prophetPost: vi.fn(),
}))

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: vi.fn(),
}))

describe("project-versions", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("listProphetProjectArtifactVersions uses pageSize and cursor in query", async () => {
    const page = {
      items: [],
      nextCursor: null,
      hasNext: false,
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: page })
    )

    const projectId = "019d0000-0000-7000-8000-000000000001"
    await listProphetProjectArtifactVersions({
      projectId,
      pageSize: 1,
      cursor: "42",
    })

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions?pageSize=1&cursor=42`
    )
  })

  it("listProphetProjectArtifactVersions throws when success false", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse(
        { success: false, messages: ["missing"] },
        { ok: false, status: 404 }
      )
    )

    await expect(
      listProphetProjectArtifactVersions({
        projectId: "019d0000-0000-7000-8000-000000000002",
      })
    ).rejects.toThrow()
  })

  it("createProphetArtifactVersion posts JSON body", async () => {
    const dto = {
      id: "019d0000-0000-7000-8000-000000000099",
      pipelineProjectId: "019d0000-0000-7000-8000-000000000001",
      versionNumber: 1,
      parentVersionId: null,
      changeSummary: null,
      pipelineStatus: "idle",
      currentStepIndex: 0,
      totalSteps: 6,
      createdAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: dto })
    )

    const projectId = "019d0000-0000-7000-8000-000000000001"
    await createProphetArtifactVersion(projectId, {})
    expect(prophetPostMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/versions`,
      { parentVersionId: null, changeSummary: null }
    )
  })
})
