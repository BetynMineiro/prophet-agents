"use client"

import { Button } from "@/components/ui/button"
import { useTranslations } from "next-intl"
import { useApiRequestStatus } from "@/lib/api/request-status/use-api-request-status"
import { toast } from "sonner"
import { Loader2Icon } from "lucide-react"
import { useEffect, useRef } from "react"
import { clearRequestError } from "@/lib/api/request-status/request-status"

type Props = Readonly<{
  hasNext: boolean
  loading: boolean
  cursor: string | null
  onNext: (cursor: string) => void
  previousLabel: string
  nextLabel: string
  moreResultsLabel: string
  endOfResultsLabel: string
}>

export function DashboardListPagination({
  hasNext,
  loading,
  cursor,
  onNext,
  previousLabel,
  nextLabel,
  moreResultsLabel,
  endOfResultsLabel,
}: Props) {
  const t = useTranslations("dashboard")
  const { error: globalError } = useApiRequestStatus()
  const lastToastErrorRef = useRef<string | null>(null)

  useEffect(() => {
    if (globalError == null) {
      lastToastErrorRef.current = null
      return
    }
    if (globalError === lastToastErrorRef.current) return
    lastToastErrorRef.current = globalError
    toast.error(t("requestFailed"))
    clearRequestError()
  }, [globalError, t])

  return (
    <div className="space-y-0">
      <div className="relative flex items-center justify-between gap-3">
        <Button variant="outline" disabled>
          {previousLabel}
        </Button>

        <div
          className="text-muted-foreground pointer-events-none absolute top-1/2 left-1/2 flex -translate-x-1/2 -translate-y-1/2 items-center justify-center gap-2 text-sm"
          aria-hidden
        >
          {loading ? (
            <Loader2Icon className="size-4 shrink-0 animate-spin" aria-hidden />
          ) : null}
          <span>{hasNext ? moreResultsLabel : endOfResultsLabel}</span>
        </div>

        <Button
          variant="outline"
          disabled={!hasNext || loading}
          onClick={() => {
            if (!cursor) return
            onNext(cursor)
          }}
        >
          {nextLabel}
        </Button>
      </div>
    </div>
  )
}
