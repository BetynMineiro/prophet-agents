"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import {
  DownloadIcon,
  EyeIcon,
  Loader2Icon,
  Trash2Icon,
  UploadIcon,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
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
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  deleteProphetProjectFinalArtifact,
  getProphetProjectFinalArtifactContent,
  getProphetProjectFinalArtifactDownloadUrl,
  listProphetProjectFinalArtifacts,
  uploadProphetProjectFinalArtifacts,
  type ProphetProjectFinalArtifactItemDto,
} from "@/lib/api/prophet"
import { MermaidMarkdown } from "@/components/features/prophet/mermaid-markdown"

const maxBytes = 5 * 1024 * 1024

function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`
  return `${(n / (1024 * 1024)).toFixed(1)} MB`
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

export function ProphetProjectFinalArtifactsSection({
  projectId,
}: Readonly<{ projectId: string }>) {
  const t = useTranslations("dashboard")
  const [items, setItems] = useState<ProphetProjectFinalArtifactItemDto[]>([])
  const [loadState, setLoadState] = useState<"loading" | "idle" | "error">(
    "loading"
  )
  const [uploading, setUploading] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] =
    useState<ProphetProjectFinalArtifactItemDto | null>(null)
  const [viewTarget, setViewTarget] =
    useState<ProphetProjectFinalArtifactItemDto | null>(null)
  const [viewMarkdown, setViewMarkdown] = useState<string | null>(null)
  const [viewLoading, setViewLoading] = useState(false)
  const fileRef = useRef<HTMLInputElement>(null)

  const load = useCallback(async () => {
    setLoadState("loading")
    try {
      const list = await listProphetProjectFinalArtifacts(projectId)
      setItems(list)
      setLoadState("idle")
    } catch {
      setLoadState("error")
    }
  }, [projectId])

  useEffect(() => {
    void Promise.resolve().then(() => load())
  }, [load])

  useEffect(() => {
    let cancelled = false
    void (async () => {
      await Promise.resolve()
      if (cancelled) return
      if (!viewTarget) {
        setViewMarkdown(null)
        return
      }
      setViewLoading(true)
      setViewMarkdown(null)
      try {
        const text = await getProphetProjectFinalArtifactContent(
          projectId,
          viewTarget.id
        )
        if (!cancelled) setViewMarkdown(text)
      } catch {
        if (!cancelled) {
          toast.error(t("prophetArtifactsViewError"))
          setViewTarget(null)
        }
      } finally {
        if (!cancelled) setViewLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [viewTarget, projectId, t])

  async function onPickFiles(e: React.ChangeEvent<HTMLInputElement>) {
    const input = e.currentTarget
    const picked = input.files ? Array.from(input.files) : []
    input.value = ""
    if (picked.length === 0) return

    const valid: File[] = []
    for (const f of picked) {
      if (!f.name.toLowerCase().endsWith(".md")) {
        toast.error(t("prophetArtifactsOnlyMd"))
        continue
      }
      if (f.size > maxBytes) {
        toast.error(t("prophetArtifactsFileTooLarge", { name: f.name }))
        continue
      }
      valid.push(f)
    }
    if (valid.length === 0) return

    setUploading(true)
    try {
      const response = await uploadProphetProjectFinalArtifacts(
        projectId,
        valid
      )
      for (const r of response.results) {
        if (r.success && r.document) {
          toast.success(t("prophetArtifactsUploadFileOk", { name: r.fileName }))
        } else {
          toast.error(
            t("prophetArtifactsUploadFileFail", {
              name: r.fileName,
              reason: r.errorMessage ?? t("prophetArtifactsUnknownError"),
            })
          )
        }
      }
      await load()
    } catch {
      toast.error(t("prophetArtifactsUploadFailed"))
    } finally {
      setUploading(false)
    }
  }

  async function onDownload(doc: ProphetProjectFinalArtifactItemDto) {
    try {
      const url = await getProphetProjectFinalArtifactDownloadUrl(
        projectId,
        doc.id
      )
      if (!url) {
        toast.error(t("prophetArtifactsDownloadFailed"))
        return
      }
      triggerBrowserDownload(url, doc.originalFileName)
    } catch {
      toast.error(t("prophetArtifactsDownloadFailed"))
    }
  }

  async function confirmDelete() {
    const doc = deleteTarget
    setDeleteTarget(null)
    if (!doc) return
    setDeletingId(doc.id)
    try {
      await deleteProphetProjectFinalArtifact(projectId, doc.id)
      toast.success(t("prophetArtifactsDeleteSuccess"))
      await load()
    } catch {
      toast.error(t("prophetArtifactsDeleteFailed"))
    } finally {
      setDeletingId(null)
    }
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
      <p className="text-destructive text-sm">
        {t("prophetArtifactsLoadFailed")}
      </p>
    )
  }

  return (
    <>
      <div className="space-y-4">
        <p className="text-muted-foreground text-sm">
          {t("prophetArtifactsDescription")}
        </p>

        <div className="flex flex-wrap items-center gap-2">
          <input
            ref={fileRef}
            type="file"
            multiple
            accept=".md,text/markdown"
            className="sr-only"
            aria-hidden
            onChange={onPickFiles}
          />
          <Button
            type="button"
            variant="outline"
            disabled={uploading}
            onClick={() => fileRef.current?.click()}
          >
            {uploading ? (
              <>
                <Loader2Icon className="mr-2 size-4 animate-spin" aria-hidden />
                {t("prophetArtifactsUploading")}
              </>
            ) : (
              <>
                <UploadIcon className="mr-2 size-4" aria-hidden />
                {t("prophetArtifactsChooseFiles")}
              </>
            )}
          </Button>
          <span className="text-muted-foreground text-xs">
            {t("prophetArtifactsMaxSizeHint")}
          </span>
        </div>

        {items.length === 0 ? (
          <p className="text-muted-foreground text-sm">
            {t("prophetArtifactsEmpty")}
          </p>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("prophetArtifactsColumnName")}</TableHead>
                  <TableHead className="w-[100px]">
                    {t("prophetArtifactsColumnSize")}
                  </TableHead>
                  <TableHead className="w-[180px]">
                    {t("prophetArtifactsColumnUploaded")}
                  </TableHead>
                  <TableHead className="w-[160px] text-end">
                    {t("prophetArtifactsColumnActions")}
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((row) => (
                  <TableRow key={row.id}>
                    <TableCell className="font-medium">
                      {row.originalFileName}
                    </TableCell>
                    <TableCell>{formatBytes(row.sizeBytes)}</TableCell>
                    <TableCell className="text-muted-foreground text-sm">
                      {new Date(row.uploadedAtUtc).toLocaleString()}
                    </TableCell>
                    <TableCell className="text-end">
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="size-8"
                        aria-label={t("prophetArtifactsView")}
                        disabled={deletingId === row.id}
                        onClick={() => setViewTarget(row)}
                      >
                        <EyeIcon className="size-4" />
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="size-8"
                        aria-label={t("prophetArtifactsDownload")}
                        disabled={deletingId === row.id}
                        onClick={() => void onDownload(row)}
                      >
                        <DownloadIcon className="size-4" />
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="text-destructive hover:text-destructive size-8"
                        aria-label={t("prophetArtifactsDelete")}
                        disabled={deletingId === row.id}
                        onClick={() => setDeleteTarget(row)}
                      >
                        {deletingId === row.id ? (
                          <Loader2Icon className="size-4 animate-spin" />
                        ) : (
                          <Trash2Icon className="size-4" />
                        )}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </div>

      <Dialog
        open={viewTarget != null}
        onOpenChange={(open) => {
          if (!open) setViewTarget(null)
        }}
      >
        <DialogContent
          showCloseButton
          className="flex max-h-[90vh] max-w-screen-2xl flex-col gap-0 overflow-hidden p-0 sm:max-w-screen-2xl"
        >
          <DialogHeader className="shrink-0 border-b px-6 py-4">
            <DialogTitle>
              {viewTarget
                ? `${t("prophetArtifactsViewTitle")}: ${viewTarget.originalFileName}`
                : t("prophetArtifactsViewTitle")}
            </DialogTitle>
          </DialogHeader>
          <div className="min-h-0 flex-1 overflow-y-auto px-6 py-4">
            {viewLoading ? (
              <div className="text-muted-foreground flex items-center gap-2 text-sm">
                <Loader2Icon className="size-4 animate-spin" aria-hidden />
                {t("prophetArtifactsViewLoading")}
              </div>
            ) : viewMarkdown != null ? (
              <MermaidMarkdown
                markdown={viewMarkdown}
                className="text-foreground text-sm"
              />
            ) : null}
          </div>
        </DialogContent>
      </Dialog>

      <AlertDialog
        open={deleteTarget != null}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetArtifactsDeleteConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetArtifactsDeleteConfirmDescription")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>
              {t("prophetDeleteCancelLabel")}
            </AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              onClick={() => void confirmDelete()}
            >
              {t("prophetDeleteConfirmAction")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
