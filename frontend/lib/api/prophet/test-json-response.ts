/**
 * Shared test helper: minimal `Response` stub for Prophet API JSON envelopes.
 */
export function prophetTestJsonResponse(
  body: unknown,
  init: { ok?: boolean; status?: number } = {}
): Response {
  const s = JSON.stringify(body)
  return {
    ok: init.ok ?? true,
    status: init.status ?? 200,
    text: () => Promise.resolve(s),
  } as Response
}
