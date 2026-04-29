"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import {
  DownloadIcon,
  ExternalLinkIcon,
  Loader2Icon,
  Trash2Icon,
  UploadIcon,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
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
  deleteProphetProjectHtmlPoc,
  getProphetProjectHtmlPocContent,
  getProphetProjectHtmlPocDownloadUrl,
  listProphetProjectHtmlPocs,
  listProphetPipelineVersionFiles,
  listProphetProjectArtifactVersions,
  uploadProphetProjectHtmlPoc,
  type ProphetPocKind,
  type ProphetPipelineVersionFileDto,
  type ProphetProjectHtmlPocItemDto,
} from "@/lib/api/prophet"

const maxBytes = 15 * 1024 * 1024

const KINDS: readonly ProphetPocKind[] = ["Web", "Mobile"]

const FILE_TYPE_BY_KIND: Record<ProphetPocKind, string> = {
  Web: "poc-web",
  Mobile: "poc-mobile",
}

function pipelineFileForKind(
  kind: ProphetPocKind,
  files: readonly ProphetPipelineVersionFileDto[]
): ProphetPipelineVersionFileDto | undefined {
  const ft = FILE_TYPE_BY_KIND[kind]
  return files.find((f) => f.fileType === ft)
}

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

/** Open synchronously from click so the popup is not blocked; `location=no` is best-effort (browsers may still show a minimal bar). */
const htmlPocPopupFeatures =
  "popup=yes,width=1280,height=800,left=80,top=80,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable=yes"

export function ProphetProjectHtmlPocsSection({
  projectId,
}: Readonly<{ projectId: string }>) {
  const t = useTranslations("dashboard")
  const [items, setItems] = useState<ProphetProjectHtmlPocItemDto[]>([])
  const [pipelineFiles, setPipelineFiles] = useState<
    ProphetPipelineVersionFileDto[]
  >([])
  const [loadState, setLoadState] = useState<"loading" | "idle" | "error">(
    "loading"
  )
  const [uploadingKind, setUploadingKind] = useState<ProphetPocKind | null>(
    null
  )
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [viewingId, setViewingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] =
    useState<ProphetProjectHtmlPocItemDto | null>(null)
  const [replaceDraft, setReplaceDraft] = useState<{
    kind: ProphetPocKind
    file: File
  } | null>(null)
  const kindRef = useRef<ProphetPocKind>("Web")
  const fileRef = useRef<HTMLInputElement>(null)

  const load = useCallback(async () => {
    setLoadState("loading")
    try {
      const list = await listProphetProjectHtmlPocs(projectId)
      setItems(list)

      let files: ProphetPipelineVersionFileDto[] = []
      try {
        const versionsPage = await listProphetProjectArtifactVersions({
          projectId,
          pageSize: 1,
        })
        const versionId = versionsPage.items[0]?.id
        if (versionId) {
          files = await listProphetPipelineVersionFiles(projectId, versionId)
        }
      } catch {
        files = []
      }
      setPipelineFiles(files)
      setLoadState("idle")
    } catch {
      setLoadState("error")
    }
  }, [projectId])

  useEffect(() => {
    void Promise.resolve().then(() => load())
  }, [load])

  function kindLabel(kind: ProphetPocKind): string {
    return kind === "Web"
      ? t("prophetHtmlPocKindWeb")
      : t("prophetHtmlPocKindMobile")
  }

  function docFor(
    kind: ProphetPocKind
  ): ProphetProjectHtmlPocItemDto | undefined {
    return items.find((i) => i.kind === kind)
  }

  async function runUpload(
    kind: ProphetPocKind,
    file: File,
    replaceConfirmed: boolean
  ) {
    setUploadingKind(kind)
    try {
      const result = await uploadProphetProjectHtmlPoc(
        projectId,
        kind,
        file,
        replaceConfirmed
      )
      if (!result.ok) {
        if (result.conflict) {
          setReplaceDraft({ kind, file })
        }
        return
      }
      toast.success(t("prophetHtmlPocUploadOk"))
      await load()
    } catch {
      toast.error(t("prophetHtmlPocUploadFailed"))
    } finally {
      setUploadingKind(null)
    }
  }

  function onPickFile(e: React.ChangeEvent<HTMLInputElement>) {
    const input = e.currentTarget
    const file = input.files?.item(0) ?? undefined
    input.value = ""
    const kind = kindRef.current
    if (!file) return

    const lower = file.name.toLowerCase()
    if (!lower.endsWith(".html") && !lower.endsWith(".htm")) {
      toast.error(t("prophetHtmlPocInvalidFile"))
      return
    }
    if (file.size > maxBytes) {
      toast.error(t("prophetHtmlPocInvalidFile"))
      return
    }

    const existing = docFor(kind)
    if (existing) {
      setReplaceDraft({ kind, file })
      return
    }
    void runUpload(kind, file, false)
  }

  function confirmReplace() {
    const draft = replaceDraft
    setReplaceDraft(null)
    if (!draft) return
    void runUpload(draft.kind, draft.file, true)
  }

  function onView(doc: ProphetProjectHtmlPocItemDto) {
    const popup = globalThis.window.open(
      "",
      `poc-${doc.id}`,
      htmlPocPopupFeatures
    )
    if (popup == null) {
      toast.error(t("prophetHtmlPocPopupBlocked"))
      return
    }

    setViewingId(doc.id)
    void (async () => {
      try {
        const html = await getProphetProjectHtmlPocContent(projectId, doc.id)
        popup.document.open()
        popup.document.write(html)
        popup.document.close()
        try {
          popup.opener = null
        } catch {
          /* ignore */
        }
      } catch {
        popup.close()
        toast.error(t("prophetHtmlPocViewFailed"))
      } finally {
        setViewingId(null)
      }
    })()
  }

  async function onDownload(doc: ProphetProjectHtmlPocItemDto) {
    try {
      const url = await getProphetProjectHtmlPocDownloadUrl(projectId, doc.id)
      if (!url) {
        toast.error(t("prophetHtmlPocDownloadFailed"))
        return
      }
      triggerBrowserDownload(url, doc.originalFileName)
    } catch {
      toast.error(t("prophetHtmlPocDownloadFailed"))
    }
  }

  function onViewPipelineFile(file: ProphetPipelineVersionFileDto) {
    const url = file.downloadUrl
    if (!url) return
    const w = globalThis.window.open(
      url,
      `_poc-pipeline-${file.id}`,
      htmlPocPopupFeatures
    )
    if (w == null) {
      toast.error(t("prophetHtmlPocPopupBlocked"))
    }
  }

  function onDownloadPipelineFile(file: ProphetPipelineVersionFileDto) {
    const url = file.downloadUrl
    if (!url) {
      toast.error(t("prophetHtmlPocDownloadFailed"))
      return
    }
    triggerBrowserDownload(url, file.originalFileName ?? "poc.html")
  }

  async function confirmDelete() {
    const doc = deleteTarget
    setDeleteTarget(null)
    if (!doc) return
    setDeletingId(doc.id)
    try {
      await deleteProphetProjectHtmlPoc(projectId, doc.id)
      toast.success(t("prophetHtmlPocDeleteOk"))
      await load()
    } catch {
      toast.error(t("prophetHtmlPocDeleteFailed"))
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
        {t("prophetHtmlPocLoadFailed")}
      </p>
    )
  }

  return (
    <>
      <p className="text-muted-foreground text-sm">
        {t("prophetHtmlPocsDescription")}
      </p>

      <input
        ref={fileRef}
        type="file"
        accept=".html,.htm,text/html"
        className="sr-only"
        aria-hidden
        onChange={onPickFile}
      />

      <div className="grid gap-4 md:grid-cols-2">
        {KINDS.map((kind) => {
          const doc = docFor(kind)
          const pipe = pipelineFileForKind(kind, pipelineFiles)
          const busy = uploadingKind === kind
          const hasManual = doc != null
          const hasPipeline =
            pipe != null && pipe.downloadUrl != null && pipe.downloadUrl !== ""

          return (
            <Card key={kind} size="sm">
              <CardHeader className="border-b pb-3">
                <CardTitle className="text-base">{kindLabel(kind)}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3 px-4 pt-2 pb-4">
                {hasManual ? (
                  <>
                    <div className="space-y-1">
                      <p className="font-medium break-all">
                        {doc.originalFileName}
                      </p>
                      <p className="text-muted-foreground text-xs">
                        {formatBytes(doc.sizeBytes)} ·{" "}
                        {new Date(doc.uploadedAtUtc).toLocaleString()}
                      </p>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        disabled={deletingId === doc.id || viewingId === doc.id}
                        onClick={() => onView(doc)}
                      >
                        {viewingId === doc.id ? (
                          <Loader2Icon
                            className="mr-1 size-4 animate-spin"
                            aria-hidden
                          />
                        ) : (
                          <ExternalLinkIcon
                            className="mr-1 size-4"
                            aria-hidden
                          />
                        )}
                        {t("prophetHtmlPocView")}
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        disabled={deletingId === doc.id}
                        onClick={() => void onDownload(doc)}
                      >
                        <DownloadIcon className="mr-1 size-4" aria-hidden />
                        {t("prophetHtmlPocDownload")}
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        disabled={deletingId === doc.id}
                        onClick={() => setDeleteTarget(doc)}
                      >
                        {deletingId === doc.id ? (
                          <Loader2Icon className="size-4 animate-spin" />
                        ) : (
                          <>
                            <Trash2Icon className="mr-1 size-4" aria-hidden />
                            {t("prophetHtmlPocDelete")}
                          </>
                        )}
                      </Button>
                    </div>
                  </>
                ) : hasPipeline && pipe ? (
                  <>
                    <div className="space-y-1">
                      <p className="font-medium break-all">
                        {pipe.originalFileName ?? "index.html"}
                      </p>
                      <p className="text-muted-foreground text-xs">
                        {t("prophetHtmlPocSourcePipeline")} ·{" "}
                        {new Date(pipe.createdAtUtc).toLocaleString()}
                      </p>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => onViewPipelineFile(pipe)}
                      >
                        <ExternalLinkIcon className="mr-1 size-4" aria-hidden />
                        {t("prophetHtmlPocView")}
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => onDownloadPipelineFile(pipe)}
                      >
                        <DownloadIcon className="mr-1 size-4" aria-hidden />
                        {t("prophetHtmlPocDownload")}
                      </Button>
                    </div>
                  </>
                ) : (
                  <p className="text-muted-foreground text-sm">
                    {t("prophetHtmlPocEmptySlot")}
                  </p>
                )}
                <div className="flex flex-wrap items-center gap-2 border-t pt-3">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={busy}
                    onClick={() => {
                      kindRef.current = kind
                      fileRef.current?.click()
                    }}
                  >
                    {busy ? (
                      <>
                        <Loader2Icon
                          className="mr-2 size-4 animate-spin"
                          aria-hidden
                        />
                        {t("prophetHtmlPocUploading")}
                      </>
                    ) : (
                      <>
                        <UploadIcon className="mr-2 size-4" aria-hidden />
                        {t("prophetHtmlPocUpload")}
                      </>
                    )}
                  </Button>
                  <span className="text-muted-foreground text-xs">
                    {t("prophetHtmlPocSlotHint")}
                  </span>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <AlertDialog
        open={replaceDraft != null}
        onOpenChange={(open) => {
          if (!open) setReplaceDraft(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetHtmlPocReplaceTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetHtmlPocReplaceDescription")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>
              {t("prophetDeleteCancelLabel")}
            </AlertDialogCancel>
            <AlertDialogAction onClick={() => confirmReplace()}>
              {t("prophetHtmlPocReplaceConfirm")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog
        open={deleteTarget != null}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prophetHtmlPocDeleteTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prophetHtmlPocDeleteDescription")}
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
