import type { Metadata } from "next"
import { hasLocale, NextIntlClientProvider } from "next-intl"
import {
  getMessages,
  getTranslations,
  setRequestLocale,
} from "next-intl/server"
import { notFound } from "next/navigation"
import { routing } from "@/i18n/routing"
import { ThemeProvider } from "@/components/shared/theme/theme-provider"
import { TooltipProvider } from "@/components/ui/tooltip"
import { Toaster } from "@/components/ui/sonner"

type Props = Readonly<{
  children: React.ReactNode
  params: Promise<{ locale: string }>
}>

export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>
}): Promise<Metadata> {
  const { locale } = await params
  const t = await getTranslations({ locale, namespace: "metadata" })
  return {
    title: t("defaultTitle"),
    description: t("defaultDescription"),
  }
}

export default async function LocaleLayout({ children, params }: Props) {
  const { locale } = await params
  if (!hasLocale(routing.locales, locale)) {
    notFound()
  }
  setRequestLocale(locale)

  const messages = await getMessages()

  return (
    <NextIntlClientProvider messages={messages}>
      <ThemeProvider>
        <TooltipProvider>
          <div className="relative flex min-h-[100dvh] min-h-svh flex-col overflow-x-hidden">
            <div className="flex-1">{children}</div>
          </div>
          <Toaster />
        </TooltipProvider>
      </ThemeProvider>
    </NextIntlClientProvider>
  )
}

export function generateStaticParams() {
  return routing.locales.map((locale) => ({ locale }))
}
