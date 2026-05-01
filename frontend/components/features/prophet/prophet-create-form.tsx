"use client"

import { useRef, useState } from "react"
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
  createProphetProject,
  type CreateProphetProjectPayload,
} from "@/lib/api/prophet"

export function ProphetCreateForm() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [expectedDate, setExpectedDate] = useState("")
  const [isActive, setIsActive] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [submitDialogOpen, setSubmitDialogOpen] = useState(false)
  const pendingSubmitRef = useRef<CreateProphetProjectPayload | null>(null)

  function isDirty(): boolean {
    return (
      name.trim() !== "" ||
      description.trim() !== "" ||
      expectedDate.trim() !== "" ||
      isActive !== true
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

  async function runCreate(payload: CreateProphetProjectPayload) {
    setIsSubmitting(true)
    try {
      await createProphetProject(payload)
      toast.success("Project created.")
      router.push("/prophet")
    } catch {
      toast.error("Could not create project.")
    } finally {
      setIsSubmitting(false)
    }
  }

  function confirmCreate() {
    const payload = pendingSubmitRef.current
    pendingSubmitRef.current = null
    if (!payload) return
    setSubmitDialogOpen(false)
    void runCreate(payload)
  }

  return (
    <>
      <form onSubmit={onSubmit} className="space-y-6">
        <div className="space-y-2">
          <Label htmlFor="prophet-create-name">Name</Label>
          <Input
            id="prophet-create-name"
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
          <Label htmlFor="prophet-create-description">Description</Label>
          <Textarea
            id="prophet-create-description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Optional"
            maxLength={4096}
            disabled={isSubmitting}
            rows={4}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="prophet-create-expected">Expected date</Label>
          <Input
            id="prophet-create-expected"
            type="date"
            value={expectedDate}
            onChange={(e) => setExpectedDate(e.target.value)}
            disabled={isSubmitting}
          />
        </div>

        <div className="flex items-center justify-between rounded-md border p-3">
          <div className="space-y-1">
            <Label htmlFor="prophet-create-active">Active</Label>
            <p className="text-muted-foreground text-sm">
              Inactive projects are hidden from the default list until reactivated.
            </p>
          </div>
          <Switch
            id="prophet-create-active"
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
                <Loader2Icon className="mr-2 size-4 animate-spin" aria-hidden />
                Creating…
              </>
            ) : (
              "Create"
            )}
          </Button>
        </div>
      </form>

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
            <AlertDialogTitle>Create this project?</AlertDialogTitle>
            <AlertDialogDescription>
              It will appear in the list. You can change it later.
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
              onClick={() => confirmCreate()}
            >
              Yes, create
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
