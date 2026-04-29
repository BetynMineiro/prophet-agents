"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import type { FormSubmitEvent } from "@/lib/types/form-submit-event"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Loader2Icon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { useRouter } from "@/i18n/navigation"
import {
  getProphetProject,
  updateProphetProject,
  type UpdateProphetProjectPayload,
} from "@/lib/api/prophet"
import { ProphetProjectFinalArtifactsSection } from "@/components/features/prophet/prophet-project-final-artifacts-section"
import { ProphetProjectHtmlPocsSection } from "@/components/features/prophet/prophet-project-html-pocs-section"
import { ProphetPipelineTimelineSection } from "@/components/features/prophet/prophet-pipeline-timeline-section"
import { ProphetRefineSection } from "@/components/features/prophet/prophet-refine-section"
import { ProphetProjectInputsSection } from "@/components/features/prophet/prophet-project-inputs-section"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs/tabs"

type InitialSnapshot = {
  name: string
  description: string
  expectedDate: string
  isActive: boolean
}

export function ProphetEditForm({
  projectId,
}: Readonly<{ projectId: string }>) {
  const t = useTranslations("dashboard")
  const router = useRouter()

  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [expectedDate, setExpectedDate] = useState("")
  const [isActive, setIsActive] = useState(true)
  const [loadState, setLoadState] = useState<"idle" | "loading" | "error">(
    "loading"
  )
  const [loadError, setLoadError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [submitDialogOpen, setSubmitDialogOpen] = useState(false)
  const pendingSubmitRef = useRef<UpdateProphetProjectPayload | null>(null)
  const initialRef = useRef<InitialSnapshot | null>(null)

  const applyLoaded = useCallback(
    (data: {
      name: string
      description: string | null
      expectedDate: string | null
      isActive: boolean
    }) => {
      const desc = (data.description ?? "").trim()
      const exp = data.expectedDate ?? ""
      setName(data.name)
      setDescription(desc)
      setExpectedDate(exp)
      setIsActive(data.isActive)
      initialRef.current = {
        name: data.name.trim(),
        description: desc,
        expectedDate: exp,
        isActive: data.isActive,
      }
    },
    []
  )

  useEffect(() => {
    let cancelled = false
    void (async () => {
      await Promise.resolve()
      if (cancelled) return
      setLoadState("loading")
      setLoadError(null)
      try {
        const data = await getProphetProject(projectId)
        if (cancelled) return
        applyLoaded(data)
        setLoadState("idle")
      } catch (e) {
        if (cancelled) return
        setLoadState("error")
        setLoadError(
          e instanceof Error ? e.message : t("prophetEditLoadFailed")
        )
      }
    })()

    return () => {
      cancelled = true
    }
  }, [applyLoaded, projectId, t])

  function isDirty(): boolean {
    const i = initialRef.current
    if (!i) return false
    return (
      name.trim() !== i.name ||
      description.trim() !== i.description ||
      expectedDate !== i.expectedDate ||
      isActive !== i.isActive
    )
  }

  function requestCancel() {
    if (!isDirty()) {
      router.push("/prophet")
      return
    }
    setCancelDialogOpen(true)
  }

  function onSubmit(e: FormSubmitEvent) {
    e.preventDefault()
    const normalizedName = name.trim()
    if (!normalizedName) {
      toast.error(t("prophetCreateValidationName"))
      return
    }
    const descTrimmed = description.trim()
    pendingSubmitRef.current = {
      name: normalizedName,
      description: descTrimmed === "" ? null : descTrimmed,
      expectedDate: expectedDate.trim() === "" ? null : expectedDate.trim(),
      isActive,
    }
    setSubmitDialogOpen(true)
  }

  async function runUpdate(payload: UpdateProphetProjectPayload) {
    setIsSubmitting(true)
    try {
      await updateProphetProject(projectId, payload)
      initialRef.current = {
        name: payload.name.trim(),
        description: (payload.description ?? "").trim(),
        expectedDate: (payload.expectedDate ?? "").trim(),
        isActive: payload.isActive,
      }
      toast.success(t("prophetEditSuccess"))
      router.push("/prophet")
    } catch {
      toast.error(t("prophetEditFailed"))
    } finally {
      setIsSubmitting(false)
    }
  }

  function confirmUpdate() {
    const payload = pendingSubmitRef.current
    pendingSubmitRef.current = null
    if (!payload) return
    setSubmitDialogOpen(false)
    void runUpdate(payload)
  }

  if (loadState === "loading") {
    return (
      <div className="text-muted-foreground flex items-center gap-2 py-8 text-sm">
        <Loader2Icon className="size-4 animate-spin" aria-hidden />
        {t("tableLoading")}
      </div>
    )
  }

  if (loadState === "error") {
    return (
      <div className="space-y-4 py-4">
        <p className="text-destructive text-sm">{loadError}</p>
        <Button
          type="button"
          variant="outline"
          onClick={() => router.push("/prophet")}
        >
          {t("prophetEditBackToList")}
        </Button>
      </div>
    )
  }

  return (
    <>
      <Tabs defaultValue="details" className="w-full">
        <TabsList
          variant="line"
          className="mb-6 flex h-auto min-h-10 w-full flex-wrap gap-1"
        >
          <TabsTrigger value="details">
            {t("prophetEditTabDetails")}
          </TabsTrigger>
          <TabsTrigger value="inputs">{t("prophetEditTabInputs")}</TabsTrigger>
          <TabsTrigger value="pipeline">
            {t("prophetEditTabPipeline")}
          </TabsTrigger>
          <TabsTrigger value="refine">{t("prophetEditTabRefine")}</TabsTrigger>
          <TabsTrigger value="artifacts">
            {t("prophetEditTabFinalArtifacts")}
          </TabsTrigger>
          <TabsTrigger value="html-pocs">
            {t("prophetEditTabHtmlPocs")}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="space-y-6">
          <form onSubmit={onSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="prophet-edit-name">
                {t("prophetCreateName")}
              </Label>
              <Input
                id="prophet-edit-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t("prophetCreateNamePlaceholder")}
                maxLength={256}
                required
                disabled={isSubmitting}
                autoComplete="off"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="prophet-edit-description">
                {t("prophetCreateDescriptionLabel")}
              </Label>
              <Textarea
                id="prophet-edit-description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder={t("prophetCreateDescriptionPlaceholder")}
                maxLength={4096}
                disabled={isSubmitting}
                rows={4}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="prophet-edit-expected">
                {t("prophetExpectedDateLabel")}
              </Label>
              <Input
                id="prophet-edit-expected"
                type="date"
                value={expectedDate}
                onChange={(e) => setExpectedDate(e.target.value)}
                disabled={isSubmitting}
              />
            </div>

            <div className="flex items-center justify-between rounded-md border p-3">
              <div className="space-y-1">
                <Label htmlFor="prophet-edit-active">
                  {t("prophetCreateActiveLabel")}
                </Label>
                <p className="text-muted-foreground text-sm">
                  {t("prophetCreateActiveHint")}
                </p>
              </div>
              <Switch
                id="prophet-edit-active"
                checked={isActive}
                onCheckedChange={setIsActive}
                disabled={isSubmitting}
              />
            </div>

            <div className="flex items-center justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={requestCancel}
                disabled={isSubmitting}
              >
                {t("prophetCreateCancel")}
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <Loader2Icon
                      className="mr-2 size-4 animate-spin"
                      aria-hidden
                    />
                    {t("prophetEditSaving")}
                  </>
                ) : (
                  t("prophetEditSave")
                )}
              </Button>
            </div>
          </form>
        </TabsContent>

        <TabsContent value="inputs" className="space-y-4">
          <ProphetProjectInputsSection projectId={projectId} />
        </TabsContent>

        <TabsContent value="pipeline" className="space-y-4">
          <ProphetPipelineTimelineSection projectId={projectId} />
        </TabsContent>

        <TabsContent value="refine" className="space-y-4">
          <ProphetRefineSection projectId={projectId} />
        </TabsContent>

        <TabsContent value="artifacts" className="space-y-4">
          <ProphetProjectFinalArtifactsSection projectId={projectId} />
        </TabsContent>

        <TabsContent value="html-pocs" className="space-y-4">
          <ProphetProjectHtmlPocsSection projectId={projectId} />
        </TabsContent>
      </Tabs>

      <AlertDialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetCancelConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetCancelConfirmDescription")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isSubmitting}>
              {t("prophetCancelConfirmStay")}
            </AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              disabled={isSubmitting}
              onClick={() => router.push("/prophet")}
            >
              {t("prophetCancelConfirmLeave")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={submitDialogOpen} onOpenChange={setSubmitDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetEditSubmitConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetEditSubmitConfirmDescription")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel
              disabled={isSubmitting}
              onClick={() => {
                pendingSubmitRef.current = null
              }}
            >
              {t("prophetEditSubmitConfirmBack")}
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={isSubmitting}
              onClick={() => confirmUpdate()}
            >
              {t("prophetEditSubmitConfirmAction")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
