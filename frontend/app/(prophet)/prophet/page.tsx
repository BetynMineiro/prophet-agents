import { PageHeader } from "@/components/layout/page-header/page-header"
import { ProphetProjectsList } from "@/components/features/prophet/prophet-projects-list"

export default function ProphetPage() {
  return (
    <main className="container mx-auto max-w-7xl space-y-8 px-4 py-8">
      <PageHeader
        title="Prophet"
        description="Prophet projects (MAF pipeline scope). List, search and manage projects for the current product."
      />
      <ProphetProjectsList />
    </main>
  )
}
