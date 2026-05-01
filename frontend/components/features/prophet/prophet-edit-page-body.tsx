"use client"

import { useSearchParams } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { ProphetEditForm } from "./prophet-edit-form"

export function ProphetEditPageBody() {
  const searchParams = useSearchParams()
  const projectId = searchParams.get("id")?.trim() ?? ""

  if (!projectId) {
    return (
      <div className="space-y-4">
        <p className="text-muted-foreground text-sm">
          No project was selected. Open edit from the list.
        </p>
        <Button variant="outline" asChild>
          <Link href="/prophet">Back to list</Link>
        </Button>
      </div>
    )
  }

  return <ProphetEditForm projectId={projectId} />
}
