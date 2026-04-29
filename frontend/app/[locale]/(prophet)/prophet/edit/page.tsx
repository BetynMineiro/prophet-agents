import { setRequestLocale } from "next-intl/server"
import { getTranslations } from "next-intl/server"
import { Suspense } from "react"
import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetEditPageBody } from "@/components/features/prophet/prophet-edit-page-body"

export default async function ProphetEditPage({
  params,
}: {
  params: Promise<{ locale: string }>
}) {
  const { locale } = await params
  setRequestLocale(locale)
  const t = await getTranslations({ locale, namespace: "dashboard" })

  return (
    <main className="container mx-auto max-w-5xl space-y-8 px-4 py-8">
      <PageHeader title={t("prophetEditTitle")} />
      <Suspense>
        <ProphetEditPageBody />
      </Suspense>
    </main>
  )
}
