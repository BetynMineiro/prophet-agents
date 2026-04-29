"use client"

import { useCallback, useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Loader2Icon } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card/card"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  listProphetPipelineArtifacts,
  type ProphetPipelineArtifactDto,
} from "@/lib/api/prophet/project-pipeline"
import { prettyJsonForMarkdown } from "@/lib/prophet/pipeline-step-preview"

export function ProphetPipelineArtifactsPanel({
  projectId,
  versionId,
  enabled,
}: Readonly<{
  projectId: string
  versionId: string | null
  enabled: boolean
}>) {
  const t = useTranslations("dashboard")
  const [items, setItems] = useState<ProphetPipelineArtifactDto[] | null>(null)
  const [loading, setLoading] = useState(false)
  const [jsonOpen, setJsonOpen] = useState(false)
  const [jsonTitle, setJsonTitle] = useState("")
  const [jsonBody, setJsonBody] = useState("")

  const load = useCallback(async () => {
    if (!versionId) return
    setLoading(true)
    try {
      const list = await listProphetPipelineArtifacts(projectId, versionId)
      setItems(list)
    } catch {
      toast.error(t("prophetPipelineArtifactsLoadFailed"))
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [projectId, versionId, t])

  useEffect(() => {
    void (async () => {
      await Promise.resolve()
      if (!enabled || !versionId) {
        setItems(null)
        return
      }
      await load()
    })()
  }, [enabled, versionId, load])

  function openJson(artifactType: string, contentJson: string) {
    setJsonTitle(artifactType)
    setJsonBody(prettyJsonForMarkdown(contentJson))
    setJsonOpen(true)
  }

  if (!enabled || !versionId) {
    return null
  }

  return (
    <>
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle className="text-base">
                {t("prophetPipelineArtifactsPanelTitle")}
              </CardTitle>
              <CardDescription>
                {t("prophetPipelineArtifactsPanelDescription")}
              </CardDescription>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={loading}
              onClick={() => void load()}
            >
              {loading ? (
                <>
                  <Loader2Icon
                    className="mr-2 size-4 shrink-0 animate-spin"
                    aria-hidden
                  />
                  {t("prophetPipelineArtifactsRefreshing")}
                </>
              ) : (
                t("prophetPipelineArtifactsRefresh")
              )}
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {loading && items == null ? (
            <div className="text-muted-foreground flex items-center gap-2 py-6 text-sm">
              <Loader2Icon className="size-4 animate-spin" aria-hidden />
              {t("prophetPipelineArtifactsLoading")}
            </div>
          ) : items != null && items.length === 0 ? (
            <p className="text-muted-foreground text-sm">
              {t("prophetPipelineArtifactsEmpty")}
            </p>
          ) : items != null ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>
                    {t("prophetPipelineArtifactsColumnType")}
                  </TableHead>
                  <TableHead className="w-[120px] text-right">
                    {t("prophetPipelineArtifactsColumnActions")}
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((row) => (
                  <TableRow key={row.artifactType}>
                    <TableCell className="font-mono text-xs">
                      {row.artifactType}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() =>
                          openJson(row.artifactType, row.contentJson)
                        }
                      >
                        {t("prophetPipelineArtifactsViewJson")}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : null}
        </CardContent>
      </Card>

      <Dialog open={jsonOpen} onOpenChange={setJsonOpen}>
        <DialogContent
          showCloseButton
          className="flex max-h-[85vh] max-w-3xl flex-col gap-0 overflow-hidden p-0 sm:max-w-3xl"
        >
          <DialogHeader className="shrink-0 border-b px-6 py-4">
            <DialogTitle className="font-mono text-sm">{jsonTitle}</DialogTitle>
          </DialogHeader>
          <div className="min-h-0 flex-1 overflow-auto px-6 py-4">
            <pre className="bg-muted/30 rounded-md border p-3 text-xs break-words whitespace-pre-wrap">
              {jsonBody}
            </pre>
          </div>
        </DialogContent>
      </Dialog>
    </>
  )
}
