import { describe, expect, it, vi, beforeEach } from "vitest"
import { readProphetApiResultJson } from "./parse-prophet-result"

const reportRequestError = vi.fn()

vi.mock("@/lib/api/request-status/request-status", () => ({
  reportRequestError: (...args: unknown[]) => reportRequestError(...args),
}))

describe("readProphetApiResultJson", () => {
  beforeEach(() => {
    reportRequestError.mockClear()
  })

  it("parses valid JSON envelope", async () => {
    const res = new Response(
      JSON.stringify({
        success: true,
        statusCode: 200,
        data: { items: [1] },
      }),
      { status: 200 }
    )
    const json = await readProphetApiResultJson<{ items: number[] }>(res)
    expect(json.success).toBe(true)
    expect(json.data?.items).toEqual([1])
  })

  it("parses failure envelope without throwing (caller checks success)", async () => {
    const res = new Response(
      JSON.stringify({
        success: false,
        statusCode: 400,
        messages: ["bad"],
        data: null,
      }),
      { status: 200 }
    )
    const json = await readProphetApiResultJson<unknown>(res)
    expect(json.success).toBe(false)
    expect(json.messages).toEqual(["bad"])
  })

  it("throws on empty body and reports error", async () => {
    const res = new Response("", { status: 404 })
    await expect(readProphetApiResultJson(res)).rejects.toThrow("HTTP 404")
    expect(reportRequestError).toHaveBeenCalled()
  })

  it("throws on whitespace-only body", async () => {
    const res = new Response("  \n  ", { status: 502 })
    await expect(readProphetApiResultJson(res)).rejects.toThrow("HTTP 502")
  })

  it("throws when body is not JSON", async () => {
    const res = new Response("<!DOCTYPE html>", { status: 200 })
    await expect(readProphetApiResultJson(res)).rejects.toThrow(
      "HTTP 200: response is not JSON"
    )
    expect(reportRequestError).toHaveBeenCalled()
  })
})
