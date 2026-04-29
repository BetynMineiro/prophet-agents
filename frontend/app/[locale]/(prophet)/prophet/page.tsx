import { setRequestLocale } from "next-intl/server"
import { getTranslations } from "next-intl/server"
import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetProjectsList } from "@/components/features/prophet/prophet-projects-list"

export default async function ProphetPage({
  params,
}: {
  params: Promise<{ locale: string }>
}) {
  const { locale } = await params
  setRequestLocale(locale)
  const t = await getTranslations({ locale, namespace: "dashboard" })

  return (
    <main className="container mx-auto max-w-7xl space-y-8 px-4 py-8">
      <PageHeader
        title={t("prophetTitle")}
        description={t("prophetDescription")}
      />
      <ProphetProjectsList />
    </main>
  )
}
