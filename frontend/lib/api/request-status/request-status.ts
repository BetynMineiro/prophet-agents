type RequestStatusSnapshot = Readonly<{
  inFlight: number
  error: string | null
}>

let snapshot: RequestStatusSnapshot = { inFlight: 0, error: null }

const listeners = new Set<() => void>()

function notify() {
  for (const l of listeners) l()
}

export function requestStarted() {
  snapshot = { ...snapshot, inFlight: snapshot.inFlight + 1, error: null }
  notify()
}

export function requestFinished() {
  snapshot = {
    ...snapshot,
    inFlight: Math.max(0, snapshot.inFlight - 1),
  }
  notify()
}

function messageFromUnknownError(error: unknown): string {
  if (error instanceof Error) return error.message
  if (typeof error === "string") return error
  return "Failed to fetch"
}

export function reportRequestError(error: unknown) {
  const message = messageFromUnknownError(error)

  snapshot = { ...snapshot, error: message }
  notify()
}

export function clearRequestError() {
  if (!snapshot.error) return
  snapshot = { ...snapshot, error: null }
  notify()
}

export function getRequestStatusSnapshot(): RequestStatusSnapshot {
  return snapshot
}

export function subscribeRequestStatus(listener: () => void): () => void {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}
