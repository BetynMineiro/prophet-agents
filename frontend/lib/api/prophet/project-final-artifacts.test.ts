import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  listProphetProjectFinalArtifacts,
  uploadProphetProjectFinalArtifacts,
  deleteProphetProjectFinalArtifact,
  getProphetProjectFinalArtifactDownloadUrl,
  getProphetProjectFinalArtifactContent,
  getProphetProjectFinalArtifactContext,
} from "./project-final-artifacts"
import { prophetTestJsonResponse } from "@/lib/api/prophet/test-json-response"
import { prophetDel, prophetGet, prophetPost } from "@/lib/api/client"

vi.mock("@/lib/api/client", () => ({
  prophetGet: vi.fn(),
  prophetPost: vi.fn(),
  prophetDel: vi.fn(),
}))

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: vi.fn(),
}))

describe("project-final-artifacts", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)
  const prophetDelMock = vi.mocked(prophetDel)

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("listProphetProjectFinalArtifacts returns data on success", async () => {
    const doc = {
      id: "019d0000-0000-7000-8000-000000000001",
      originalFileName: "a.md",
      contentType: "text/markdown",
      sizeBytes: 3,
      uploadedAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: [doc] })
    )

    const pid = "019d0000-0000-7000-8000-000000000002"
    const result = await listProphetProjectFinalArtifacts(pid)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/final-artifacts`
    )
    expect(result).toEqual([doc])
  })

  it("uploadProphetProjectFinalArtifacts posts FormData with files field", async () => {
    const upload = {
      results: [
        {
          fileName: "x.md",
          success: true,
          errorMessage: null,
          document: {
            id: "019d0000-0000-7000-8000-000000000099",
            originalFileName: "x.md",
            contentType: "text/markdown",
            sizeBytes: 2,
            uploadedAtUtc: "2026-01-01T00:00:00Z",
          },
        },
      ],
    }
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: upload })
    )

    const pid = "019d0000-0000-7000-8000-000000000003"
    const file = new File([new Uint8Array([1, 2])], "x.md", {
      type: "text/markdown",
    })
    const result = await uploadProphetProjectFinalArtifacts(pid, [file])

    expect(prophetPostMock).toHaveBeenCalledTimes(1)
    const [, body] = prophetPostMock.mock.calls[0]
    expect(body).toBeInstanceOf(FormData)
    expect(result).toEqual(upload)
  })

  it("deleteProphetProjectFinalArtifact calls delete endpoint", async () => {
    prophetDelMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true })
    )

    const pid = "019d0000-0000-7000-8000-000000000004"
    const did = "019d0000-0000-7000-8000-000000000005"
    await deleteProphetProjectFinalArtifact(pid, did)

    expect(prophetDelMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/final-artifacts/${did}`
    )
  })

  it("getProphetProjectFinalArtifactDownloadUrl returns url", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { downloadUrl: "https://signed.example/o" },
      })
    )

    const pid = "019d0000-0000-7000-8000-000000000006"
    const did = "019d0000-0000-7000-8000-000000000007"
    const url = await getProphetProjectFinalArtifactDownloadUrl(pid, did)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/final-artifacts/${did}/download`
    )
    expect(url).toBe("https://signed.example/o")
  })

  it("getProphetProjectFinalArtifactContent returns text from content endpoint", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: { text: "# Doc\n" } })
    )

    const pid = "019d0000-0000-7000-8000-000000000008"
    const did = "019d0000-0000-7000-8000-000000000009"
    const text = await getProphetProjectFinalArtifactContent(pid, did)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/final-artifacts/${did}/content`
    )
    expect(text).toBe("# Doc\n")
  })

  it("getProphetProjectFinalArtifactContext is an alias for getProphetProjectFinalArtifactContent", () => {
    expect(getProphetProjectFinalArtifactContext).toBe(
      getProphetProjectFinalArtifactContent
    )
  })
})
