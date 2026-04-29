/** Avoids jsdom "Not implemented: navigation to another Document" when tests touch `location` (e.g. auth redirects). */
import "vitest-location-mock"
import "@testing-library/jest-dom/vitest"
import { afterAll, afterEach, beforeAll } from "vitest"
import { setupServer } from "msw/node"

/** MSW server for tests that register handlers; others are unaffected. */
const mswServer = setupServer()

beforeAll(() => {
  mswServer.listen({ onUnhandledRequest: "warn" })
})

afterEach(() => {
  mswServer.resetHandlers()
})

afterAll(() => {
  mswServer.close()
})

// Radix Select / menus call Pointer Capture APIs not implemented in jsdom.
if (typeof Element.prototype.hasPointerCapture !== "function") {
  Element.prototype.hasPointerCapture = () => false
}
if (typeof Element.prototype.setPointerCapture !== "function") {
  Element.prototype.setPointerCapture = () => {}
}
if (typeof Element.prototype.releasePointerCapture !== "function") {
  Element.prototype.releasePointerCapture = () => {}
}

Element.prototype.scrollIntoView ??= function scrollIntoViewPolyfill() {
  void 0
}

// Radix Tooltip / Popper uses ResizeObserver (not in jsdom).
globalThis.ResizeObserver ??= class ResizeObserver {
  observe(): void {
    void 0
  }
  unobserve(): void {
    void 0
  }
  disconnect(): void {
    void 0
  }
}

// Some components/utilities access the `window` global directly.
// Ensure it always exists (even if a test runner momentarily lacks it).
if (globalThis.window === undefined) {
  ;(globalThis as unknown as { window?: typeof globalThis }).window = globalThis
}
