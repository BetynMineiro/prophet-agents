"use client"

import {
  Fragment,
  useCallback,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from "react"
import { toast } from "sonner"
import { DownloadIcon, Loader2Icon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card/card"
import { Badge } from "@/components/ui/badge/badge"
import { Label } from "@/components/ui/label"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  continueProphetPipeline,
  createProphetArtifactVersion,
  getProphetPipelineRunStatus,
  listProphetProjectArtifactVersions,
  listProphetProjectInputs,
  rewindProphetPipelineToStep,
  retryProphetPipelineStep,
  runProphetPipeline,
} from "@/lib/api/prophet"
import { mapProphetPipelineRunStatusToUi } from "@/lib/prophet/map-prophet-pipeline-api-status"
import {
  prophetPipelineStatusAllowsContinue,
  prophetPipelineStatusAllowsFullRun,
  prophetPipelineStatusAllowsInteractiveActions,
} from "@/lib/prophet/pipeline-run-helpers"
import { shouldScheduleAutoContinue } from "@/lib/prophet/pipeline-auto-continue-breakpoints"
import {
  loadPipelineStepPreview,
  type PipelineStepPreviewResult,
} from "@/lib/prophet/pipeline-step-preview"
import {
  type PipelineStepUiStatus,
  type ProphetPipelineStatusDto,
} from "@/lib/prophet/pipeline-ui-model"
import { MermaidMarkdown } from "@/components/features/prophet/mermaid-markdown"
import {
  chunkStepsWithStartIndex,
  computePipelineColumns,
  statusBadgeVariant,
} from "@/components/features/prophet/pipeline-timeline/pipeline-timeline-layout"
import {
  PipelineRow,
  RowVerticalBridge,
} from "@/components/features/prophet/pipeline-timeline/pipeline-timeline-row"
import { ProphetPipelineArtifactsPanel } from "@/components/features/prophet/prophet-pipeline-artifacts-panel"

const STEP_LABELS: Record<string, string> = {
  file: "File",
  insight: "Insight",
  market: "Market",
  model: "Model",
  architecture: "Architecture",
  diagram: "Diagram",
  "poc-web": "Web POC",
  "poc-mobile": "Mobile POC",
  doc: "Documentation",
  packaging: "Packaging",
}

const STEP_STATUS_LABELS: Record<string, string> = {
  completed: "Completed",
  running: "Running",
  pending: "Pending",
  waiting: "Waiting",
  failed: "Failed",
}

export function ProphetPipelineTimelineSection({
  projectId,
}: Readonly<{ projectId: string }>) {
  const [apiLoadState, setApiLoadState] = useState<
    "idle" | "loading" | "ready" | "error" | "empty"
  >("idle")
  const [livePipeline, setLivePipeline] =
    useState<ProphetPipelineStatusDto | null>(null)
  const [liveVersionId, setLiveVersionId] = useState<string | null>(null)

  const [previewOpen, setPreviewOpen] = useState(false)
  const [previewStepIndex, setPreviewStepIndex] = useState<number | null>(null)
  const [previewStepLabel, setPreviewStepLabel] = useState("")
  const [previewMarkdown, setPreviewMarkdown] = useState<string | null>(null)
  const [previewEmbed, setPreviewEmbed] = useState<{
    url: string
    fileName: string
  } | null>(null)
  const [previewLoading, setPreviewLoading] = useState(false)

  const diagramRef = useRef<HTMLDivElement>(null)
  const [columns, setColumns] = useState(5)
  const [pipelineActionBusy, setPipelineActionBusy] = useState(false)
  const [runInteractive, setRunInteractive] = useState(false)
  /** Step indices where auto-continue stops for manual Continue / Retry / Rewind (only when Interactive is on). */
  const [autoContinuePauseAtSteps, setAutoContinuePauseAtSteps] = useState<
    ReadonlySet<number>
  >(() => new Set())

  const toggleAutoContinuePauseAtStep = useCallback((stepIndex: number) => {
    setAutoContinuePauseAtSteps((prev) => {
      const next = new Set(prev)
      if (next.has(stepIndex)) next.delete(stepIndex)
      else next.add(stepIndex)
      return next
    })
  }, [])

  const timelineData: ProphetPipelineStatusDto | null =
    apiLoadState === "ready" && livePipeline != null ? livePipeline : null

  const loadLivePipeline = useCallback(async () => {
    setApiLoadState("loading")
    setLivePipeline(null)
    setLiveVersionId(null)
    try {
      const page = await listProphetProjectArtifactVersions({
        projectId,
        pageSize: 1,
      })
      let vid: string
      if (page.items.length === 0) {
        const projectInputs = await listProphetProjectInputs(projectId)
        if (projectInputs.length === 0) {
          setApiLoadState("empty")
          return
        }
        const created = await createProphetArtifactVersion(projectId, {})
        vid = created.id
      } else {
        vid = page.items[0].id
      }
      const status = await getProphetPipelineRunStatus(projectId, vid)
      setLiveVersionId(vid)
      setLivePipeline(mapProphetPipelineRunStatusToUi(status))
      setApiLoadState("ready")
    } catch {
      setApiLoadState("error")
      toast.error("Could not load pipeline status.")
    }
  }, [projectId])

  const refreshLiveStatus = useCallback(async () => {
    if (!liveVersionId) return
    try {
      const status = await getProphetPipelineRunStatus(projectId, liveVersionId)
      setLivePipeline(mapProphetPipelineRunStatusToUi(status))
    } catch {
      /* polling soft-fail */
    }
  }, [projectId, liveVersionId])

  /** While a run/continue/retry/rewind request is in flight, the server is often already `Running` but the UI still showed `Paused` — poll so the timeline shows the spinner. */
  const pipelineStatusLower =
    livePipeline?.pipelineStatus.trim().toLowerCase() ?? ""
  const shouldPollPipelineStatus =
    liveVersionId != null &&
    livePipeline != null &&
    (pipelineStatusLower === "running" || pipelineActionBusy)

  useEffect(() => {
    if (!shouldPollPipelineStatus) return
    const intervalMs = pipelineActionBusy ? 1500 : 3000
    const id = setInterval(() => {
      void refreshLiveStatus()
    }, intervalMs)
    return () => clearInterval(id)
  }, [shouldPollPipelineStatus, pipelineActionBusy, refreshLiveStatus])

  useEffect(() => {
    if (!pipelineActionBusy || !liveVersionId) return
    void Promise.resolve().then(() => void refreshLiveStatus())
  }, [pipelineActionBusy, liveVersionId, refreshLiveStatus])

  useEffect(() => {
    void Promise.resolve().then(() => void loadLivePipeline())
  }, [loadLivePipeline])

  useLayoutEffect(() => {
    const el = diagramRef.current
    if (!el) return

    function measure() {
      const node = diagramRef.current
      if (!node) return
      setColumns(computePipelineColumns(node.getBoundingClientRect().width))
    }

    measure()
    const ro = new ResizeObserver(() => {
      measure()
    })
    ro.observe(el)
    return () => {
      ro.disconnect()
    }
  }, [timelineData])

  const pipelineRows = useMemo(() => {
    if (timelineData == null) return []
    return chunkStepsWithStartIndex(timelineData.steps, columns)
  }, [timelineData, columns])

  function stepLabel(stepId: string): string {
    return STEP_LABELS[stepId] ?? stepId
  }

  function stepStatusLabel(status: PipelineStepUiStatus): string {
    return STEP_STATUS_LABELS[status] ?? status
  }

  function canOpenStepPreview(
    step: ProphetPipelineStatusDto["steps"][number]
  ): boolean {
    if (step.status !== "completed") return false
    return apiLoadState === "ready" && liveVersionId != null
  }

  function triggerBrowserDownload(url: string, fileName: string): void {
    const a = document.createElement("a")
    a.href = url
    a.setAttribute("download", fileName)
    a.rel = "noopener"
    document.body.appendChild(a)
    a.click()
    a.remove()
  }

  function downloadStepPreview() {
    if (previewStepIndex == null) return
    if (previewEmbed != null) {
      triggerBrowserDownload(previewEmbed.url, previewEmbed.fileName)
      return
    }
    if (previewMarkdown == null) return
    const safeLabel = previewStepLabel
      .replace(/[^\p{L}\p{N}_-]+/gu, "_")
      .replace(/^_+|_+$/g, "")
      .slice(0, 80)
    const base =
      safeLabel.length > 0
        ? `step-${previewStepIndex}-${safeLabel}`
        : `step-${previewStepIndex}`
    const fileName = `${base}.md`
    const blob = new Blob([previewMarkdown], {
      type: "text/markdown;charset=utf-8",
    })
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = fileName
    a.rel = "noopener"
    a.click()
    URL.revokeObjectURL(url)
  }

  function openStepPreview(stepIndex: number) {
    if (timelineData == null) return
    const step = timelineData.steps[stepIndex]
    if (!canOpenStepPreview(step)) return
    setPreviewStepIndex(stepIndex)
    setPreviewStepLabel(stepLabel(step.stepId))
    setPreviewOpen(true)
    setPreviewLoading(true)
    setPreviewMarkdown(null)
    setPreviewEmbed(null)
    void (async () => {
      try {
        if (liveVersionId != null) {
          const result: PipelineStepPreviewResult =
            await loadPipelineStepPreview(projectId, liveVersionId, stepIndex)
          if (result.mode === "iframe") {
            setPreviewEmbed({
              url: result.url,
              fileName: result.fileName,
            })
            setPreviewMarkdown(null)
          } else {
            setPreviewMarkdown(result.markdown)
            setPreviewEmbed(null)
          }
        }
      } catch {
        toast.error("Could not load this step's preview.")
        setPreviewOpen(false)
        setPreviewStepIndex(null)
      } finally {
        setPreviewLoading(false)
      }
    })()
  }

  const continuePipeline = useCallback(
    async (options?: { silent?: boolean }) => {
      if (!liveVersionId) return
      setPipelineActionBusy(true)
      try {
        await continueProphetPipeline(projectId, liveVersionId)
        if (!options?.silent) {
          toast.success("Continued to the next step.")
        }
        await refreshLiveStatus()
      } catch (e) {
        if (!options?.silent) {
          toast.error(
            e instanceof Error ? e.message : "Continue failed."
          )
        }
      } finally {
        setPipelineActionBusy(false)
      }
    },
    [liveVersionId, projectId, refreshLiveStatus]
  )

  /** When Interactive is on and the server leaves the pipeline `Paused`, call Continue automatically until Completed / Failed — except at steps marked "pause for review". */
  useEffect(() => {
    if (!liveVersionId) return
    if (pipelineActionBusy) return
    if (
      !shouldScheduleAutoContinue({
        runInteractive,
        pipelineStatusLower,
        currentStepIndex: livePipeline?.currentStepIndex ?? -1,
        pauseAtStepIndices: autoContinuePauseAtSteps,
      })
    )
      return

    const id = globalThis.window.setTimeout(() => {
      void continuePipeline({ silent: true })
    }, 200)
    return () => globalThis.window.clearTimeout(id)
  }, [
    runInteractive,
    liveVersionId,
    livePipeline?.currentStepIndex,
    pipelineActionBusy,
    pipelineStatusLower,
    continuePipeline,
    autoContinuePauseAtSteps,
  ])

  async function onRetry(stepIndex: number) {
    if (!liveVersionId) return
    setPipelineActionBusy(true)
    try {
      await retryProphetPipelineStep(projectId, liveVersionId, stepIndex)
      toast.success(`Retry started for step ${stepIndex}.`)
      await refreshLiveStatus()
    } catch (e) {
      toast.error(
        e instanceof Error ? e.message : "Retry failed."
      )
    } finally {
      setPipelineActionBusy(false)
    }
  }

  async function onRewind(stepIndex: number) {
    if (!liveVersionId) return
    setPipelineActionBusy(true)
    try {
      await rewindProphetPipelineToStep(projectId, liveVersionId, stepIndex)
      toast.success(`Rewound to step ${stepIndex}.`)
      await refreshLiveStatus()
    } catch (e) {
      toast.error(
        e instanceof Error ? e.message : "Rewind failed."
      )
    } finally {
      setPipelineActionBusy(false)
    }
  }

  async function onRunPipeline() {
    if (!liveVersionId) return
    setPipelineActionBusy(true)
    try {
      await runProphetPipeline(projectId, liveVersionId, {
        interactiveSteps: runInteractive,
      })
      toast.success("Pipeline run finished.")
      await refreshLiveStatus()
    } catch (e) {
      toast.error(
        e instanceof Error ? e.message : "Pipeline run failed."
      )
    } finally {
      setPipelineActionBusy(false)
    }
  }

  const interactiveActionsDisabled =
    livePipeline == null ||
    !prophetPipelineStatusAllowsInteractiveActions(
      livePipeline.pipelineStatus
    ) ||
    pipelineActionBusy

  const canRunPipeline =
    liveVersionId != null &&
    livePipeline != null &&
    prophetPipelineStatusAllowsFullRun(livePipeline.pipelineStatus)

  const runDisabled = !canRunPipeline || pipelineActionBusy

  const interactiveSwitchDisabled =
    pipelineActionBusy || pipelineStatusLower === "running"

  const canContinuePipeline =
    liveVersionId != null &&
    livePipeline != null &&
    prophetPipelineStatusAllowsContinue(livePipeline.pipelineStatus)

  const continueDisabled = !canContinuePipeline || pipelineActionBusy

  return (
    <div className="space-y-6">
      <p className="text-muted-foreground text-sm">
        Interactive pipeline: preview step states and actions before wiring the live API.
      </p>

      <div
        className="rounded-md border border-dashed border-sky-600/35 bg-sky-500/5 px-3 py-2 text-sm text-sky-950 dark:text-sky-50/90"
        role="status"
      >
        Live API — status of the project&apos;s latest artifact version. Preview for completed steps reads that version&apos;s artifacts and files.
      </div>

      <div className="flex flex-col gap-4">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-end">
            <Button
              type="button"
              variant="secondary"
              size="sm"
              disabled={apiLoadState === "loading"}
              onClick={() => void loadLivePipeline()}
            >
              {apiLoadState === "loading" ? (
                <>
                  <Loader2Icon
                    className="mr-2 size-4 shrink-0 animate-spin"
                    aria-hidden
                  />
                  Loading…
                </>
              ) : (
                "Refresh status"
              )}
            </Button>
          </div>
        </div>

        {apiLoadState === "ready" && liveVersionId != null ? (
          <div
            className="border-border/70 bg-muted/20 flex flex-col gap-3 rounded-lg border p-4 sm:flex-row sm:flex-wrap sm:items-center"
            role="region"
            aria-label="Live pipeline controls"
          >
            <Button
              type="button"
              variant="default"
              size="sm"
              disabled={runDisabled}
              onClick={() => void onRunPipeline()}
            >
              {pipelineActionBusy ? (
                <Loader2Icon
                  className="mr-2 size-4 shrink-0 animate-spin"
                  aria-hidden
                />
              ) : null}
              Run pipeline
            </Button>
            <div className="flex items-center gap-2">
              <Switch
                id="prophet-pipeline-interactive"
                checked={runInteractive}
                onCheckedChange={setRunInteractive}
                disabled={interactiveSwitchDisabled}
              />
              <Label
                htmlFor="prophet-pipeline-interactive"
                className="text-sm font-normal"
              >
                Interactive (auto-continue each step)
              </Label>
            </div>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              disabled={continueDisabled}
              onClick={() => void continuePipeline()}
            >
              Continue
            </Button>
            <p className="text-muted-foreground w-full text-xs sm:w-auto sm:flex-1">
              Run pipeline: restarts from step 1 when status is Idle, Failed, or Completed (clears prior outputs). Interactive: after each step the client calls Continue automatically until the run finishes (toggle anytime except while Running). With Interactive on, use &quot;Pause for review&quot; on steps where you want to stop for manual Continue, Retry, or Rewind; after Continue, auto-continue runs again until the next pause or completion. When Paused with Interactive off, use Continue manually. Rewind here is also available when Completed — the pipeline becomes Paused at that step. Add source files under the project Inputs tab.
            </p>
          </div>
        ) : null}
      </div>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Pipeline status</CardTitle>
          <CardDescription>
            Version:{" "}
            <span className="font-mono text-xs">
              {timelineData?.versionId ?? "—"}
            </span>
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {apiLoadState === "loading" ? (
            <div className="text-muted-foreground flex items-center gap-2 py-10 text-sm">
              <Loader2Icon
                className="size-5 shrink-0 animate-spin"
                aria-hidden
              />
              Loading…
            </div>
          ) : apiLoadState === "empty" ? (
            <p className="text-muted-foreground text-sm">
              This project has no artifact versions yet.
            </p>
          ) : timelineData == null ? (
            <p className="text-muted-foreground text-sm">
              Could not load pipeline status.
            </p>
          ) : timelineData != null ? (
            <>
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-muted-foreground text-sm">Status:</span>
                <Badge
                  variant={statusBadgeVariant(timelineData.pipelineStatus)}
                >
                  {timelineData.pipelineStatus}
                </Badge>
                <span className="text-muted-foreground text-sm">
                  · Step:{" "}
                  {timelineData.currentStepIndex} / {timelineData.totalSteps}
                </span>
              </div>
              {timelineData.error ? (
                <p className="border-destructive/40 bg-destructive/5 text-destructive rounded-md border px-3 py-2 text-sm">
                  {timelineData.error}
                </p>
              ) : null}

              <div className="space-y-2">
                <p className="text-muted-foreground text-xs">
                  Columns adjust to the available width (minimum card size). On very narrow screens you may scroll a row sideways. With API data source, per-step Retry/Rewind call Prophet; with Mock, they show toasts only.
                </p>
                <p className="text-muted-foreground text-xs">
                  Intermediate output can only be previewed when the step completed successfully.
                </p>
                <p className="text-muted-foreground text-xs">
                  Run again re-executes that step immediately (Paused, Failed, or Completed). Rewind here only moves the pointer to that step and leaves the pipeline Paused — then use Continue or Run pipeline as needed.
                </p>
                <div
                  ref={diagramRef}
                  className="border-border/80 from-muted/30 to-muted/10 dark:from-muted/20 dark:to-muted/5 relative min-w-0 rounded-xl border bg-gradient-to-b p-4"
                  role="region"
                  aria-label="Pipeline flow diagram"
                >
                  <div className="flex flex-col gap-3">
                    {pipelineRows.map((row, ri) => (
                      <Fragment key={row.startIndex}>
                        <PipelineRow
                          columns={columns}
                          steps={row.steps}
                          startIndex={row.startIndex}
                          labelStep={stepLabel}
                          labelStatus={stepStatusLabel}
                          onRetry={onRetry}
                          onRewind={onRewind}
                          onPreviewStep={openStepPreview}
                          previewLabel="Preview output"
                          previewHelpTitle="Only available for successfully completed steps."
                          canOpenPreview={canOpenStepPreview}
                          retryLabel="Run again"
                          rewindLabel="Rewind here"
                          retryHelpTitle="Clears this step's output and runs it immediately (POST …/steps/retry)."
                          rewindHelpTitle="Moves the pipeline to this step and stays paused until you press Continue (POST …/steps/rewind)."
                          interactiveActionsDisabled={
                            interactiveActionsDisabled
                          }
                          showAutoContinueBreakpoints={runInteractive}
                          autoContinueBreakpointIndices={
                            autoContinuePauseAtSteps
                          }
                          onToggleAutoContinueBreakpoint={
                            toggleAutoContinuePauseAtStep
                          }
                          autoContinueBreakpointLabel="Pause for review"
                          autoContinueBreakpointTitle="While Interactive is on, stop auto-continue before this step runs so you can inspect output or use Retry/Rewind. Press Continue to resume auto-continue until the next pause or the end."
                        />
                        {ri < pipelineRows.length - 1 ? (
                          <RowVerticalBridge />
                        ) : null}
                      </Fragment>
                    ))}
                  </div>
                </div>
              </div>
            </>
          ) : null}
        </CardContent>
      </Card>

      <ProphetPipelineArtifactsPanel
        projectId={projectId}
        versionId={liveVersionId}
        enabled={apiLoadState === "ready" && liveVersionId != null}
      />

      <Dialog
        open={previewOpen}
        onOpenChange={(open) => {
          setPreviewOpen(open)
          if (!open) {
            setPreviewStepIndex(null)
            setPreviewStepLabel("")
            setPreviewMarkdown(null)
            setPreviewEmbed(null)
            setPreviewLoading(false)
          }
        }}
      >
        <DialogContent
          showCloseButton
          className="flex max-h-[85vh] max-w-3xl flex-col gap-0 overflow-hidden p-0 sm:max-w-3xl"
        >
          <DialogHeader className="shrink-0 space-y-0 border-b px-6 py-4">
            <div className="flex flex-wrap items-center justify-between gap-3 pr-10">
              <DialogTitle className="text-start">
                {previewStepIndex != null && previewStepLabel.length > 0
                  ? `Step output: ${previewStepLabel}`
                  : "Preview output"}
              </DialogTitle>
              {!previewLoading &&
              (previewMarkdown != null || previewEmbed != null) ? (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="shrink-0"
                  onClick={() => downloadStepPreview()}
                >
                  <DownloadIcon className="mr-2 size-4 shrink-0" aria-hidden />
                  Download result
                </Button>
              ) : null}
            </div>
          </DialogHeader>
          <div className="min-h-0 flex-1 overflow-y-auto px-6 py-4">
            {previewLoading ? (
              <div className="text-muted-foreground flex items-center gap-2 py-8 text-sm">
                <Loader2Icon
                  className="size-5 shrink-0 animate-spin"
                  aria-hidden
                />
                Loading content…
              </div>
            ) : previewEmbed != null ? (
              <div className="flex h-[min(70vh,720px)] w-full min-w-0 flex-col gap-2">
                <p className="text-muted-foreground truncate text-xs">
                  {previewEmbed.fileName}
                </p>
                <iframe
                  title={previewStepLabel || "poc-preview"}
                  src={previewEmbed.url}
                  className="border-border bg-background min-h-[420px] w-full flex-1 rounded-md border"
                  sandbox="allow-scripts allow-same-origin allow-forms allow-popups"
                />
              </div>
            ) : previewMarkdown != null ? (
              <MermaidMarkdown
                className="text-foreground text-sm"
                markdown={previewMarkdown}
              />
            ) : null}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
