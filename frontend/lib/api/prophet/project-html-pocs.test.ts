import { describe, expect, it, vi, beforeEach } from "vitest"
import {
  listProphetProjectHtmlPocs,
  getProphetProjectHtmlPocContent,
  getProphetProjectHtmlPocDownloadUrl,
  getProphetProjectHtmlPocViewUrl,
  uploadProphetProjectHtmlPoc,
  deleteProphetProjectHtmlPoc,
} from "./project-html-pocs"
import { prophetTestJsonResponse } from "@/lib/api/prophet/test-json-response"
import { prophetGet, prophetPost, prophetDel } from "@/lib/api/client"

vi.mock("@/lib/api/client", () => ({
  prophetGet: vi.fn(),
  prophetPost: vi.fn(),
  prophetDel: vi.fn(),
}))

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: vi.fn(),
}))

describe("project-html-pocs", () => {
  const prophetGetMock = vi.mocked(prophetGet)
  const prophetPostMock = vi.mocked(prophetPost)
  const prophetDelMock = vi.mocked(prophetDel)

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("listProphetProjectHtmlPocs returns data on success", async () => {
    const doc = {
      id: "019d0000-0000-7000-8000-000000000001",
      kind: "Web" as const,
      originalFileName: "a.html",
      contentType: "text/html",
      sizeBytes: 3,
      uploadedAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: [doc] })
    )

    const pid = "019d0000-0000-7000-8000-000000000002"
    const result = await listProphetProjectHtmlPocs(pid)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/html-pocs`
    )
    expect(result).toEqual([doc])
  })

  it("listProphetProjectHtmlPocs normalizes numeric kind to Mobile", async () => {
    const raw = {
      id: "019d0000-0000-7000-8000-000000000011",
      kind: 1,
      originalFileName: "m.html",
      contentType: "text/html",
      sizeBytes: 1,
      uploadedAtUtc: "2026-01-01T00:00:00Z",
    }
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: [raw] })
    )
    const pid = "019d0000-0000-7000-8000-000000000012"
    const result = await listProphetProjectHtmlPocs(pid)
    expect(result[0].kind).toBe("Mobile")
  })

  it("uploadProphetProjectHtmlPoc returns ok document on success", async () => {
    const doc = {
      id: "019d0000-0000-7000-8000-000000000013",
      kind: "Web" as const,
      originalFileName: "a.html",
      contentType: "text/html",
      sizeBytes: 2,
      uploadedAtUtc: "2026-01-01T00:00:00Z",
    }
    const file = new File(["<!doctype html>"], "a.html", {
      type: "text/html",
    })
    prophetPostMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: doc })
    )
    const pid = "019d0000-0000-7000-8000-000000000014"
    const result = await uploadProphetProjectHtmlPoc(pid, "Web", file, false)
    expect(result).toEqual({ ok: true, document: doc })
    const [, body] = prophetPostMock.mock.calls[0]!
    expect(body).toBeInstanceOf(FormData)
  })

  it("uploadProphetProjectHtmlPoc returns conflict when 409 poc_kind_already_exists", async () => {
    prophetPostMock.mockResolvedValueOnce({
      ok: false,
      status: 409,
      text: () =>
        Promise.resolve(
          JSON.stringify({
            success: false,
            messages: ["poc_kind_already_exists"],
          })
        ),
    } as Response)
    const file = new File(["x"], "a.html", { type: "text/html" })
    const pid = "019d0000-0000-7000-8000-000000000015"
    const result = await uploadProphetProjectHtmlPoc(pid, "Web", file, true)
    expect(result).toEqual({ ok: false, conflict: true })
  })

  it("uploadProphetProjectHtmlPoc throws on empty body", async () => {
    prophetPostMock.mockResolvedValueOnce({
      ok: false,
      status: 500,
      text: () => Promise.resolve("   "),
    } as Response)
    const file = new File(["x"], "a.html", { type: "text/html" })
    await expect(
      uploadProphetProjectHtmlPoc(
        "019d0000-0000-7000-8000-000000000016",
        "Web",
        file,
        false
      )
    ).rejects.toThrow()
  })

  it("uploadProphetProjectHtmlPoc throws when response is not JSON", async () => {
    prophetPostMock.mockResolvedValueOnce({
      ok: false,
      status: 500,
      text: () => Promise.resolve("not json"),
    } as Response)
    const file = new File(["x"], "a.html", { type: "text/html" })
    await expect(
      uploadProphetProjectHtmlPoc(
        "019d0000-0000-7000-8000-000000000017",
        "Web",
        file,
        false
      )
    ).rejects.toThrow(/not JSON/)
  })

  it("deleteProphetProjectHtmlPoc resolves on success", async () => {
    prophetDelMock.mockResolvedValueOnce(
      prophetTestJsonResponse({ success: true, data: true })
    )
    const pid = "019d0000-0000-7000-8000-000000000018"
    const did = "019d0000-0000-7000-8000-000000000019"
    await deleteProphetProjectHtmlPoc(pid, did)
    expect(prophetDelMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/html-pocs/${did}`
    )
  })

  it("getProphetProjectHtmlPocDownloadUrl returns url", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { downloadUrl: "https://signed.example/dl" },
      })
    )
    const pid = "019d0000-0000-7000-8000-000000000020"
    const did = "019d0000-0000-7000-8000-000000000021"
    const url = await getProphetProjectHtmlPocDownloadUrl(pid, did)
    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/html-pocs/${did}/download`
    )
    expect(url).toBe("https://signed.example/dl")
  })

  it("getProphetProjectHtmlPocViewUrl returns url", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { downloadUrl: "https://signed.example/view" },
      })
    )

    const pid = "019d0000-0000-7000-8000-000000000006"
    const did = "019d0000-0000-7000-8000-000000000007"
    const url = await getProphetProjectHtmlPocViewUrl(pid, did)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/html-pocs/${did}/view`
    )
    expect(url).toBe("https://signed.example/view")
  })

  it("getProphetProjectHtmlPocContent returns HTML text", async () => {
    prophetGetMock.mockResolvedValueOnce(
      prophetTestJsonResponse({
        success: true,
        data: { text: "<!DOCTYPE html><html></html>" },
      })
    )

    const pid = "019d0000-0000-7000-8000-000000000008"
    const did = "019d0000-0000-7000-8000-000000000009"
    const html = await getProphetProjectHtmlPocContent(pid, did)

    expect(prophetGetMock).toHaveBeenCalledWith(
      `/v1/prophet/projects/${pid}/html-pocs/${did}/content`
    )
    expect(html).toBe("<!DOCTYPE html><html></html>")
  })
})
