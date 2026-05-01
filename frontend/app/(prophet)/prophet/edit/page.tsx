import { Suspense } from "react"
import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetEditPageBody } from "@/components/features/prophet/prophet-edit-page-body"

export default function ProphetEditPage() {
  return (
    <main className="container mx-auto max-w-5xl space-y-8 px-4 py-8">
      <PageHeader title="Edit Prophet project" />
      <Suspense>
        <ProphetEditPageBody />
      </Suspense>
    </main>
  )
}
