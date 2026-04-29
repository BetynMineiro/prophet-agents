"use client"

import { useSearchParams } from "next/navigation"
import { useTranslations } from "next-intl"
import { Link } from "@/i18n/navigation"
import { Button } from "@/components/ui/button"
import { ProphetEditForm } from "./prophet-edit-form"

export function ProphetEditPageBody() {
  const searchParams = useSearchParams()
  const t = useTranslations("dashboard")
  const projectId = searchParams.get("id")?.trim() ?? ""

  if (!projectId) {
    return (
      <div className="space-y-4">
        <p className="text-muted-foreground text-sm">
          {t("prophetEditMissingId")}
        </p>
        <Button variant="outline" asChild>
          <Link href="/prophet">{t("prophetEditBackToList")}</Link>
        </Button>
      </div>
    )
  }

  return <ProphetEditForm projectId={projectId} />
}
