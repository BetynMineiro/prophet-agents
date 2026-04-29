import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  listProphetProjectInputs,
  uploadProphetProjectInputs,
  deleteProphetProjectInput,
  getProphetProjectInputDownloadUrl,
} from "./project-inputs"
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

describe("project-inputs", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)
  const prophetDelMock = vi.mocked(prophetDel)

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("listProphetProjectInputs returns data on success", async () => {
    const doc = {
      id: "019d0000-0000-7000-8000-000000000001",
      originalFileName: "a.txt",
      contentType: "text/plain",
      sizeBytes: 3,
      uploadedAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: [doc] })
    )

    const pid = "019d0000-0000-7000-8000-000000000002"
    const result = await listProphetProjectInputs(pid)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/inputs`
    )
    expect(result).toEqual([doc])
  })

  it("listProphetProjectInputs throws when success false", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse(
        { success: false, messages: ["nope"] },
        { ok: false, status: 400 }
      )
    )

    await expect(
      listProphetProjectInputs("019d0000-0000-7000-8000-000000000002")
    ).rejects.toThrow()
  })

  it("uploadProphetProjectInputs posts FormData with files field", async () => {
    const upload = {
      results: [
        {
          fileName: "x.txt",
          success: true,
          errorMessage: null,
          document: {
            id: "019d0000-0000-7000-8000-000000000099",
            originalFileName: "x.txt",
            contentType: "text/plain",
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
    const file = new File([new Uint8Array([1, 2])], "x.txt", {
      type: "text/plain",
    })
    const result = await uploadProphetProjectInputs(pid, [file])

    expect(prophetPostMock).toHaveBeenCalledTimes(1)
    const [, body] = prophetPostMock.mock.calls[0]
    expect(body).toBeInstanceOf(FormData)
    expect(result).toEqual(upload)
  })

  it("deleteProphetProjectInput calls delete endpoint", async () => {
    prophetDelMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true })
    )

    const pid = "019d0000-0000-7000-8000-000000000004"
    const did = "019d0000-0000-7000-8000-000000000005"
    await deleteProphetProjectInput(pid, did)

    expect(prophetDelMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/inputs/${did}`
    )
  })

  it("getProphetProjectInputDownloadUrl returns url", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { downloadUrl: "https://signed.example/o" },
      })
    )

    const pid = "019d0000-0000-7000-8000-000000000006"
    const did = "019d0000-0000-7000-8000-000000000007"
    const url = await getProphetProjectInputDownloadUrl(pid, did)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/inputs/${did}/download`
    )
    expect(url).toBe("https://signed.example/o")
  })
})
