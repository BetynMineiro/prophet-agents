import { useSyncExternalStore } from "react"
import {
  getRequestStatusSnapshot,
  subscribeRequestStatus,
} from "./request-status"

export function useApiRequestStatus() {
  return useSyncExternalStore(
    subscribeRequestStatus,
    getRequestStatusSnapshot,
    getRequestStatusSnapshot
  )
}
