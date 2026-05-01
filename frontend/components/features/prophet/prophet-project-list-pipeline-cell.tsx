"use client"

import type { ReactNode } from "react"
import {
  CheckCircle2,
  Circle,
  Loader2,
  PauseCircle,
  XCircle,
} from "lucide-react"
import { cn } from "@/lib/core/utils"
import { formatProphetProjectListPipelineStatus } from "@/lib/prophet/format-project-list-pipeline-status"

type Props = {
  status: string | null | undefined
}

/**
 * Pipeline status for one grid row: icon + label (Running = blue spinner, Failed red, Paused orange, Completed green).
 */
export function ProphetProjectListPipelineCell({ status }: Props) {
  const label = formatProphetProjectListPipelineStatus(status)
  const raw = status?.trim() ?? ""
  const low = raw.toLowerCase()

  if (!raw) {
    return (
      <span className="text-muted-foreground" title={label}>
        {label}
      </span>
    )
  }

  const row = (icon: ReactNode, className: string) => (
    <span
      className={cn("inline-flex items-center gap-1.5", className)}
      title={label}
    >
      {icon}
      <span className="font-medium">{label}</span>
    </span>
  )

  switch (low) {
    case "running":
      return row(
        <Loader2
          className="size-4 shrink-0 animate-spin text-blue-600 dark:text-blue-400"
          aria-hidden
        />,
        "text-blue-700 dark:text-blue-300"
      )
    case "failed":
      return row(
        <XCircle
          className="size-4 shrink-0 text-red-600 dark:text-red-400"
          aria-hidden
        />,
        "text-red-700 dark:text-red-300"
      )
    case "paused":
      return row(
        <PauseCircle
          className="size-4 shrink-0 text-amber-600 dark:text-amber-400"
          aria-hidden
        />,
        "text-amber-800 dark:text-amber-200"
      )
    case "completed":
      return row(
        <CheckCircle2
          className="size-4 shrink-0 text-emerald-600 dark:text-emerald-400"
          aria-hidden
        />,
        "text-emerald-800 dark:text-emerald-300"
      )
    case "idle":
      return row(
        <Circle
          className="text-muted-foreground size-4 shrink-0"
          aria-hidden
        />,
        "text-muted-foreground"
      )
    default:
      return (
        <span className="text-muted-foreground" title={label}>
          {label}
        </span>
      )
  }
}
