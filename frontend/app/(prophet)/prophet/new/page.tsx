import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetCreateForm } from "@/components/features/prophet/prophet-create-form"

export default function ProphetNewPage() {
  return (
    <main className="container mx-auto max-w-3xl space-y-8 px-4 py-8">
      <PageHeader title="New Prophet project" />
      <ProphetCreateForm />
    </main>
  )
}
