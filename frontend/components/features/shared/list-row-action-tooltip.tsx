"use client"

import type { ReactElement } from "react"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"

type Props = Readonly<{
  /** Visible on hover (should match or complement aria-label). */
  label: string
  children: ReactElement
}>

/**
 * Tooltip for icon actions in tables (edit, etc.).
 * Root layout already provides TooltipProvider.
 */
export function ListRowActionTooltip({ label, children }: Props) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>{children}</TooltipTrigger>
      <TooltipContent side="top" sideOffset={6}>
        {label}
      </TooltipContent>
    </Tooltip>
  )
}
