import { setRequestLocale } from "next-intl/server"
import { getTranslations } from "next-intl/server"
import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetCreateForm } from "@/components/features/prophet/prophet-create-form"

export default async function ProphetNewPage({
  params,
}: {
  params: Promise<{ locale: string }>
}) {
  const { locale } = await params
  setRequestLocale(locale)
  const t = await getTranslations({ locale, namespace: "dashboard" })

  return (
    <main className="container mx-auto max-w-3xl space-y-8 px-4 py-8">
      <PageHeader title={t("prophetCreateTitle")} />
      <ProphetCreateForm />
    </main>
  )
}
