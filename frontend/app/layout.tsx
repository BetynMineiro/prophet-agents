import { Inter, Geist_Mono } from "next/font/google"
import type { Metadata } from "next"
import "./globals.css"
import { cn } from "@/lib/core/utils"
import { ThemeProvider } from "@/components/shared/theme/theme-provider"
import { TooltipProvider } from "@/components/ui/tooltip"
import { Toaster } from "@/components/ui/sonner"

const inter = Inter({ subsets: ["latin"], variable: "--font-sans" })
const fontMono = Geist_Mono({
  subsets: ["latin"],
  variable: "--font-mono",
})

export const metadata: Metadata = {
  title: "Prophet",
  description: "Prophet AI/ML pipeline",
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html
      lang="en"
      suppressHydrationWarning
      className={cn(
        "antialiased",
        fontMono.variable,
        "font-sans",
        inter.variable
      )}
    >
      <body className="min-h-[100dvh] min-h-svh antialiased">
        <ThemeProvider>
          <TooltipProvider>
            <div className="relative flex min-h-[100dvh] min-h-svh flex-col overflow-x-hidden">
              <div className="flex-1">{children}</div>
            </div>
            <Toaster />
          </TooltipProvider>
        </ThemeProvider>
      </body>
    </html>
  )
}
