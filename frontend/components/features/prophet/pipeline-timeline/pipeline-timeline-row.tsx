"use client"

import { Fragment } from "react"
import {
  AlertCircleIcon,
  CheckCircle2Icon,
  ChevronDownIcon,
  ChevronRightIcon,
  CircleIcon,
  EyeIcon,
  Loader2Icon,
  PauseCircleIcon,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge/badge"
import { cn } from "@/lib/core/utils"
import type {
  PipelineStepUiStatus,
  ProphetPipelineStatusDto,
} from "@/lib/prophet/pipeline-ui-model"
import { PIPELINE_CONNECTOR_PX } from "@/components/features/prophet/pipeline-timeline/pipeline-timeline-constants"
import {
  pipelineCardFlexStyle,
  stepBoxTone,
} from "@/components/features/prophet/pipeline-timeline/pipeline-timeline-layout"

function StepIcon({ status }: { status: PipelineStepUiStatus }) {
  switch (status) {
    case "completed":
      return (
        <CheckCircle2Icon
          className="size-5 shrink-0 text-emerald-600 dark:text-emerald-400"
          aria-hidden
        />
      )
    case "running":
      return (
        <Loader2Icon
          className="text-primary size-5 shrink-0 animate-spin"
          aria-hidden
        />
      )
    case "failed":
      return (
        <AlertCircleIcon
          className="text-destructive size-5 shrink-0"
          aria-hidden
        />
      )
    case "waiting":
      return (
        <PauseCircleIcon
          className="size-5 shrink-0 text-amber-600 dark:text-amber-400"
          aria-hidden
        />
      )
    case "pending":
    default:
      return (
        <CircleIcon
          className="text-muted-foreground size-5 shrink-0"
          aria-hidden
        />
      )
  }
}

export function HorizontalConnector() {
  return (
    <div
      className="flex shrink-0 flex-col items-center justify-center self-stretch px-0.5"
      style={{
        width: PIPELINE_CONNECTOR_PX,
        minWidth: PIPELINE_CONNECTOR_PX,
      }}
      aria-hidden
    >
      <div className="flex items-center">
        <div className="bg-border h-px w-1.5 sm:w-2" />
        <ChevronRightIcon className="text-muted-foreground/70 size-4 shrink-0" />
      </div>
    </div>
  )
}

export function RowVerticalBridge() {
  return (
    <div className="flex justify-center py-1" aria-hidden>
      <div className="text-muted-foreground/70 flex flex-col items-center gap-0.5">
        <div className="bg-border h-3 w-px" />
        <ChevronDownIcon className="size-4 shrink-0" />
        <div className="bg-border h-3 w-px" />
      </div>
    </div>
  )
}

export function PipelineRow({
  columns,
  steps,
  startIndex,
  labelStep,
  labelStatus,
  onRetry,
  onRewind,
  onPreviewStep,
  previewLabel,
  previewHelpTitle,
  canOpenPreview,
  retryLabel,
  rewindLabel,
  retryHelpTitle,
  rewindHelpTitle,
  interactiveActionsDisabled = false,
  showAutoContinueBreakpoints = false,
  autoContinueBreakpointIndices,
  onToggleAutoContinueBreakpoint,
  autoContinueBreakpointLabel,
  autoContinueBreakpointTitle,
}: Readonly<{
  columns: number
  steps: ProphetPipelineStatusDto["steps"]
  startIndex: number
  labelStep: (stepId: string) => string
  labelStatus: (status: PipelineStepUiStatus) => string
  onRetry: (stepIndex: number) => void
  onRewind: (stepIndex: number) => void
  onPreviewStep: (stepIndex: number) => void
  previewLabel: string
  previewHelpTitle: string
  canOpenPreview: (step: ProphetPipelineStatusDto["steps"][number]) => boolean
  retryLabel: string
  rewindLabel: string
  retryHelpTitle: string
  rewindHelpTitle: string
  /** When true, retry/rewind are disabled (e.g. pipeline not paused). */
  interactiveActionsDisabled?: boolean
  /** When Interactive (auto-continue) is on: show a checkbox to pause before running this step. */
  showAutoContinueBreakpoints?: boolean
  autoContinueBreakpointIndices?: ReadonlySet<number>
  onToggleAutoContinueBreakpoint?: (stepIndex: number) => void
  autoContinueBreakpointLabel?: string
  autoContinueBreakpointTitle?: string
}>) {
  const cardFlex = pipelineCardFlexStyle(columns)

  return (
    <div className="min-w-0 overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:thin] [&::-webkit-scrollbar]:h-1.5">
      <div className="flex w-full min-w-0 items-stretch justify-start gap-0 px-0.5 py-1">
        {steps.map((step, i) => {
          const index = startIndex + i
          return (
            <Fragment key={step.stepId}>
              <div
                style={cardFlex}
                className={cn(
                  "flex min-h-0 min-w-0 flex-col rounded-xl border p-3 shadow-sm transition-colors",
                  stepBoxTone(step.status)
                )}
              >
                <div className="mb-2 flex items-start justify-between gap-2">
                  <span className="bg-background/80 text-muted-foreground ring-border flex size-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold tabular-nums ring-1">
                    {index + 1}
                  </span>
                  <StepIcon status={step.status} />
                </div>
                <p
                  className="line-clamp-2 min-h-[2.5rem] text-sm leading-tight font-medium"
                  title={labelStep(step.stepId)}
                >
                  {labelStep(step.stepId)}
                </p>
                <p className="text-muted-foreground mt-0.5 truncate font-mono text-[10px]">
                  {step.stepId}
                </p>
                <Badge
                  variant="outline"
                  className="mt-2 w-fit max-w-full truncate text-[10px]"
                >
                  {labelStatus(step.status)}
                </Badge>
                {showAutoContinueBreakpoints &&
                autoContinueBreakpointIndices != null &&
                onToggleAutoContinueBreakpoint != null &&
                autoContinueBreakpointLabel != null ? (
                  <label
                    className="border-border/60 bg-muted/30 text-muted-foreground mt-2 flex cursor-pointer items-start gap-2 rounded-md border px-2 py-1.5 text-left text-[11px] leading-snug"
                    title={autoContinueBreakpointTitle ?? undefined}
                  >
                    <input
                      type="checkbox"
                      className="border-input accent-primary mt-0.5 size-3.5 shrink-0 rounded"
                      checked={autoContinueBreakpointIndices.has(index)}
                      onChange={() => onToggleAutoContinueBreakpoint(index)}
                      aria-label={`${autoContinueBreakpointLabel} (${labelStep(step.stepId)})`}
                    />
                    <span className="min-w-0 flex-1">
                      {autoContinueBreakpointLabel}
                    </span>
                  </label>
                ) : null}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="mt-2 h-8 w-full gap-1.5 text-xs"
                  title={previewHelpTitle}
                  disabled={!canOpenPreview(step)}
                  onClick={() => onPreviewStep(index)}
                >
                  <EyeIcon className="size-3.5 shrink-0" aria-hidden />
                  {previewLabel}
                </Button>
                <div className="border-border/60 mt-3 flex flex-col gap-1.5 border-t pt-3">
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    className="h-8 w-full text-xs"
                    title={retryHelpTitle}
                    disabled={interactiveActionsDisabled}
                    onClick={() => onRetry(index)}
                  >
                    {retryLabel}
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="h-8 w-full text-xs"
                    title={rewindHelpTitle}
                    disabled={interactiveActionsDisabled}
                    onClick={() => onRewind(index)}
                  >
                    {rewindLabel}
                  </Button>
                </div>
              </div>
              {i < steps.length - 1 ? <HorizontalConnector /> : null}
            </Fragment>
          )
        })}
      </div>
    </div>
  )
}
