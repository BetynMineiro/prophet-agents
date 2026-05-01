"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import type { FormSubmitEvent } from "@/lib/types/form-submit-event"
import { useRouter } from "next/navigation"
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
import {
  getProphetProject,
  updateProphetProject,
  type UpdateProphetProjectPayload,
} from "@/lib/api/prophet"
import { ProphetPipelineTimelineSection } from "@/components/features/prophet/prophet-pipeline-timeline-section"
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
          e instanceof Error ? e.message : "Could not load project."
        )
      }
    })()

    return () => {
      cancelled = true
    }
  }, [applyLoaded, projectId])

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
      toast.error("Name is required.")
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
      toast.success("Project updated.")
      router.push("/prophet")
    } catch {
      toast.error("Could not update project.")
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
        Loading...
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
          Back to list
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
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="inputs">Input documents</TabsTrigger>
          <TabsTrigger value="pipeline">Pipeline</TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="space-y-6">
          <form onSubmit={onSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="prophet-edit-name">Name</Label>
              <Input
                id="prophet-edit-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Project name"
                maxLength={256}
                required
                disabled={isSubmitting}
                autoComplete="off"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="prophet-edit-description">Description</Label>
              <Textarea
                id="prophet-edit-description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Optional"
                maxLength={4096}
                disabled={isSubmitting}
                rows={4}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="prophet-edit-expected">Expected date</Label>
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
                <Label htmlFor="prophet-edit-active">Active</Label>
                <p className="text-muted-foreground text-sm">
                  Inactive projects are hidden from the default list until reactivated.
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
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <Loader2Icon
                      className="mr-2 size-4 animate-spin"
                      aria-hidden
                    />
                    Saving…
                  </>
                ) : (
                  "Save changes"
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
      </Tabs>

      <AlertDialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Discard changes?</AlertDialogTitle>
            <AlertDialogDescription>
              Any information you entered will be lost.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isSubmitting}>
              Keep editing
            </AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              disabled={isSubmitting}
              onClick={() => router.push("/prophet")}
            >
              Leave
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={submitDialogOpen} onOpenChange={setSubmitDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Save changes?</AlertDialogTitle>
            <AlertDialogDescription>
              The project will be updated with the values you entered.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel
              disabled={isSubmitting}
              onClick={() => {
                pendingSubmitRef.current = null
              }}
            >
              Back
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={isSubmitting}
              onClick={() => confirmUpdate()}
            >
              Yes, save
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
