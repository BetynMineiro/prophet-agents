import { Inter, Geist_Mono } from "next/font/google"
import type { Metadata } from "next"
import "./globals.css"
import { cn } from "@/lib/core/utils"

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
      lang="pt"
      suppressHydrationWarning
      className={cn(
        "antialiased",
        fontMono.variable,
        "font-sans",
        inter.variable
      )}
    >
      <body className="min-h-[100dvh] min-h-svh antialiased">{children}</body>
    </html>
  )
}
