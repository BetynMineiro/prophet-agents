"use client"

import { useState, type FormEvent } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Loader2Icon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card/card"
import { refineProphetProject } from "@/lib/api/prophet"

export function ProphetRefineSection({
  projectId,
}: Readonly<{ projectId: string }>) {
  const t = useTranslations("dashboard")
  const [changeRequest, setChangeRequest] = useState("")
  const [submitting, setSubmitting] = useState(false)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    const text = changeRequest.trim()
    if (!text) {
      toast.error(t("prophetRefineValidation"))
      return
    }
    setSubmitting(true)
    try {
      const dto = await refineProphetProject(projectId, {
        change_request: text,
      })
      toast.success(
        t("prophetRefineSuccess", {
          version: dto.versionNumber,
          step: dto.startFromStepIndex,
        })
      )
      setChangeRequest("")
    } catch {
      toast.error(t("prophetRefineFailed"))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("prophetRefineTitle")}</CardTitle>
        <CardDescription>{t("prophetRefineDescription")}</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="prophet-refine-request">
              {t("prophetRefineLabel")}
            </Label>
            <Textarea
              id="prophet-refine-request"
              value={changeRequest}
              onChange={(e) => setChangeRequest(e.target.value)}
              placeholder={t("prophetRefinePlaceholder")}
              rows={6}
              maxLength={8000}
              disabled={submitting}
            />
            <p className="text-muted-foreground text-xs">
              {t("prophetRefineHint")}
            </p>
          </div>
          <Button type="submit" disabled={submitting}>
            {submitting ? (
              <>
                <Loader2Icon className="mr-2 size-4 animate-spin" aria-hidden />
                {t("prophetRefineSubmitting")}
              </>
            ) : (
              t("prophetRefineSubmit")
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}
