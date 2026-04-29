"use client"

import { Button } from "@/components/ui/button"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import type { LucideIcon } from "lucide-react"
import { Trash2 as Trash2Icon } from "lucide-react"

type Props = Readonly<{
  disabled: boolean
  ariaLabel: string
  /** Tooltip text; defaults to ariaLabel. */
  tooltip?: string
  confirmTitle: string
  confirmDescription: string
  cancelLabel: string
  confirmActionLabel: string
  onConfirm: () => void
  /** Defaults to trash (delete/deactivate). */
  Icon?: LucideIcon
  /** When false, confirm uses default button style (e.g. restore). */
  confirmDestructive?: boolean
}>

export function ConfirmDeleteAction({
  disabled,
  ariaLabel,
  tooltip,
  confirmTitle,
  confirmDescription,
  cancelLabel,
  confirmActionLabel,
  onConfirm,
  Icon = Trash2Icon,
  confirmDestructive = true,
}: Props) {
  const tooltipLabel = tooltip ?? ariaLabel

  return (
    <AlertDialog>
      <Tooltip>
        <TooltipTrigger asChild>
          <AlertDialogTrigger asChild>
            <Button
              type="button"
              variant="ghost"
              size="icon-xs"
              className="text-muted-foreground hover:text-foreground"
              aria-label={ariaLabel}
              disabled={disabled}
            >
              <Icon className="size-4" />
            </Button>
          </AlertDialogTrigger>
        </TooltipTrigger>
        <TooltipContent side="top" sideOffset={6}>
          {tooltipLabel}
        </TooltipContent>
      </Tooltip>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{confirmTitle}</AlertDialogTitle>
          <AlertDialogDescription>{confirmDescription}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={disabled}>
            {cancelLabel}
          </AlertDialogCancel>
          <AlertDialogAction
            variant={confirmDestructive ? "destructive" : "default"}
            disabled={disabled}
            onClick={onConfirm}
          >
            {confirmActionLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
