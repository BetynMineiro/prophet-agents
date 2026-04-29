"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { DownloadIcon, Loader2Icon, Trash2Icon, UploadIcon } from "lucide-react"
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
  deleteProphetProjectInput,
  getProphetProjectInputDownloadUrl,
  listProphetProjectInputs,
  uploadProphetProjectInputs,
  type ProphetProjectInputDocumentItemDto,
} from "@/lib/api/prophet"

const maxBytes = 10 * 1024 * 1024

function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`
  return `${(n / (1024 * 1024)).toFixed(1)} MB`
}

/** Triggers save/download in the browser (no new tab). Works with signed URLs that send Content-Disposition: attachment. */
function triggerBrowserDownload(url: string, fileName: string): void {
  const a = document.createElement("a")
  a.href = url
  a.setAttribute("download", fileName)
  a.rel = "noopener"
  document.body.appendChild(a)
  a.click()
  a.remove()
}

export function ProphetProjectInputsSection({
  projectId,
}: Readonly<{ projectId: string }>) {
  const t = useTranslations("dashboard")
  const [items, setItems] = useState<ProphetProjectInputDocumentItemDto[]>([])
  const [loadState, setLoadState] = useState<"loading" | "idle" | "error">(
    "loading"
  )
  const [uploading, setUploading] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] =
    useState<ProphetProjectInputDocumentItemDto | null>(null)
  const fileRef = useRef<HTMLInputElement>(null)

  const load = useCallback(async () => {
    setLoadState("loading")
    try {
      const list = await listProphetProjectInputs(projectId)
      setItems(list)
      setLoadState("idle")
    } catch {
      setLoadState("error")
    }
  }, [projectId])

  useEffect(() => {
    void Promise.resolve().then(() => load())
  }, [load])

  async function onPickFiles(e: React.ChangeEvent<HTMLInputElement>) {
    const input = e.currentTarget
    // Capture File before clearing — `FileList` is live; resetting value empties it.
    const file = input.files?.item(0) ?? undefined
    input.value = ""
    if (!file) return

    if (file.size > maxBytes) {
      toast.error(t("prophetInputsFileTooLarge", { name: file.name }))
      return
    }

    setUploading(true)
    try {
      const response = await uploadProphetProjectInputs(projectId, [file])
      for (const r of response.results) {
        if (r.success && r.document) {
          toast.success(t("prophetInputsUploadFileOk", { name: r.fileName }))
        } else {
          toast.error(
            t("prophetInputsUploadFileFail", {
              name: r.fileName,
              reason: r.errorMessage ?? t("prophetInputsUnknownError"),
            })
          )
        }
      }
      await load()
    } catch {
      toast.error(t("prophetInputsUploadFailed"))
    } finally {
      setUploading(false)
    }
  }

  async function onDownload(doc: ProphetProjectInputDocumentItemDto) {
    try {
      const url = await getProphetProjectInputDownloadUrl(projectId, doc.id)
      if (!url) {
        toast.error(t("prophetInputsDownloadFailed"))
        return
      }
      triggerBrowserDownload(url, doc.originalFileName)
    } catch {
      toast.error(t("prophetInputsDownloadFailed"))
    }
  }

  async function confirmDelete() {
    const doc = deleteTarget
    setDeleteTarget(null)
    if (!doc) return
    setDeletingId(doc.id)
    try {
      await deleteProphetProjectInput(projectId, doc.id)
      toast.success(t("prophetInputsDeleteSuccess"))
      await load()
    } catch {
      toast.error(t("prophetInputsDeleteFailed"))
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
      <p className="text-destructive text-sm">{t("prophetInputsLoadFailed")}</p>
    )
  }

  return (
    <>
      <div className="space-y-4">
        <p className="text-muted-foreground text-sm">
          {t("prophetInputsDescription")}
        </p>

        <div className="flex flex-wrap items-center gap-2">
          <input
            ref={fileRef}
            type="file"
            accept=".pdf,.doc,.docx,.txt,.md,.markdown,.json,.xml,.yaml,.yml,.csv,.html,.htm,.css,.sql,.ts,.tsx,.js,.jsx,.cs,.py,.java,.go,.rs,.c,.h,.cpp,.hpp,.vue,.svelte,text/plain,text/markdown,application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
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
                {t("prophetInputsUploading")}
              </>
            ) : (
              <>
                <UploadIcon className="mr-2 size-4" aria-hidden />
                {t("prophetInputsChooseFile")}
              </>
            )}
          </Button>
          <span className="text-muted-foreground text-xs">
            {t("prophetInputsMaxSizeHint")}
          </span>
        </div>

        {items.length === 0 ? (
          <p className="text-muted-foreground text-sm">
            {t("prophetInputsEmpty")}
          </p>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("prophetInputsColumnName")}</TableHead>
                  <TableHead className="w-[100px]">
                    {t("prophetInputsColumnSize")}
                  </TableHead>
                  <TableHead className="w-[180px]">
                    {t("prophetInputsColumnUploaded")}
                  </TableHead>
                  <TableHead className="w-[120px] text-end">
                    {t("prophetInputsColumnActions")}
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
                        aria-label={t("prophetInputsDownload")}
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
                        aria-label={t("prophetInputsDelete")}
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

      <AlertDialog
        open={deleteTarget != null}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetInputsDeleteConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetInputsDeleteConfirmDescription")}
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
