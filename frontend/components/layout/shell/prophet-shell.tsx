"use client"

import { useTheme } from "next-themes"
import { MoonIcon, SunIcon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Link } from "@/i18n/navigation"

function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme()

  return (
    <Button
      variant="ghost"
      size="icon"
      aria-label="Toggle theme"
      onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
    >
      {resolvedTheme === "dark" ? (
        <SunIcon className="size-4" />
      ) : (
        <MoonIcon className="size-4" />
      )}
    </Button>
  )
}

export function ProphetShell({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <div className="flex min-h-[100dvh] min-h-svh flex-col">
      <header className="bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-40 border-b backdrop-blur">
        <div className="container mx-auto flex h-14 max-w-7xl items-center justify-between px-4">
          <Link
            href="/prophet"
            className="text-foreground hover:text-foreground/80 text-lg font-semibold tracking-tight"
          >
            Prophet
          </Link>
          <ThemeToggle />
        </div>
      </header>
      <div className="flex-1">{children}</div>
    </div>
  )
}
