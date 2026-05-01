import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  createProphetProject,
  deleteProphetProject,
  getProphetProject,
  getProphetProjects,
  restoreProphetProject,
  updateProphetProject,
} from "./projects"
import { prophetTestJsonResponse } from "@/lib/api/prophet/test-json-response"
import {
  prophetDel,
  prophetGet,
  prophetPatch,
  prophetPost,
  prophetPut,
} from "@/lib/api/client"

vi.mock("@/lib/api/client", () => ({
  prophetGet: vi.fn(),
  prophetPost: vi.fn(),
  prophetPut: vi.fn(),
  prophetDel: vi.fn(),
  prophetPatch: vi.fn(),
}))

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: vi.fn(),
}))

describe("projects", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)
  const prophetPutMock = vi.mocked(prophetPut)
  const prophetDelMock = vi.mocked(prophetDel)
  const prophetPatchMock = vi.mocked(prophetPatch)
  const projectId = "019d0000-0000-7000-8000-000000000001"

  const sampleProject = {
    id: projectId,
    name: "P",
    description: null,
    expectedDate: null,
    createdAtUtc: "2026-01-01T00:00:00Z",
    isActive: true,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("getProphetProjects builds query with searchText and activeState", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: [sampleProject] })
    )

    const result = await getProphetProjects({
      searchText: "x",
      activeState: "All",
    })
    expect(prophetGetMock).toHaveBeenCalledWith(
      expect.stringMatching(
        /\/v1\/prophet\/projects\?searchText=x&activeState=All/
      )
    )
    expect(result).toEqual([sampleProject])
  })

  it("getProphetProject returns data on success", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: sampleProject })
    )
    await expect(getProphetProject(projectId)).resolves.toEqual(sampleProject)
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}`
    )
  })

  it("getProphetProject throws on 404 envelope", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse(
        { success: false, messages: ["gone"] },
        { ok: false, status: 404 }
      )
    )
    await expect(getProphetProject(projectId)).rejects.toThrow()
  })

  it("createProphetProject posts payload", async () => {
    const payload = {
      name: "N",
      description: "d",
      expectedDate: "2026-02-01",
      isActive: true,
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: sampleProject })
    )
    await expect(createProphetProject(payload)).resolves.toEqual(sampleProject)
    expect(prophetPostMock).toHaveBeenCalledWith(
      "/v1/prophet/projects",
      payload
    )
  })

  it("updateProphetProject puts payload", async () => {
    const payload = {
      name: "N2",
      description: null,
      expectedDate: null,
      isActive: false,
    }
    prophetPutMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: sampleProject })
    )
    await expect(updateProphetProject(projectId, payload)).resolves.toEqual(
      sampleProject
    )
    expect(prophetPutMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}`,
      payload
    )
  })

  it("deleteProphetProject returns ApiResult from client", async () => {
    const apiResult = { success: true, data: true }
    prophetDelMock.mockResolvedValueOnce(prophetTestJsonResponse(apiResult))
    await expect(deleteProphetProject(projectId)).resolves.toEqual(apiResult)
    expect(prophetDelMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}`
    )
  })

  it("restoreProphetProject patches restore", async () => {
    prophetPatchMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: sampleProject })
    )
    await expect(restoreProphetProject(projectId)).resolves.toEqual(
      sampleProject
    )
    expect(prophetPatchMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${projectId}/restore`
    )
  })
})
