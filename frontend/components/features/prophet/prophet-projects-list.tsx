"use client"

import { useCallback, useEffect, useRef, useState } from "react"
import {
  deleteProphetProject,
  getProphetProjects,
  restoreProphetProject,
  type ProphetProjectItemDto,
} from "@/lib/api/prophet"
import { normalizeSearchText } from "@/lib/api/normalize-search-text"
import {
  ActiveState as ActiveStateValue,
  type ActiveState,
} from "@/lib/api/active-state"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { PencilIcon, RotateCcwIcon } from "lucide-react"
import { ListRowActionTooltip } from "../shared/list-row-action-tooltip"
import {
  editableTableRowA11yProps,
  stopRowDoubleClickNavigation,
} from "../shared/list-row-double-click"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table/table"
import { DashboardListToolbar } from "../shared/list-toolbar"
import { ConfirmDeleteAction } from "../shared/confirm-delete-action"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { ProphetProjectListPipelineCell } from "@/components/features/prophet/prophet-project-list-pipeline-cell"
import { cn } from "@/lib/core/utils"

function formatCreatedAt(iso: string): string {
  try {
    return new Date(iso).toLocaleString(undefined, {
      dateStyle: "short",
      timeStyle: "short",
    })
  } catch {
    return iso
  }
}

function formatExpectedDate(isoDate: string | null): string {
  if (!isoDate) return "—"
  try {
    const [y, m, d] = isoDate.split("-").map(Number)
    if (!y || !m || !d) return isoDate
    return new Date(y, m - 1, d).toLocaleDateString(undefined, {
      dateStyle: "short",
    })
  } catch {
    return isoDate
  }
}

export function ProphetProjectsList() {
  const router = useRouter()
  const [items, setItems] = useState<ProphetProjectItemDto[]>([])
  const [loading, setLoading] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [restoringId, setRestoringId] = useState<string | null>(null)

  const [searchText, setSearchText] = useState("")
  const debouncedSearch = useDebouncedValue(searchText, 400)
  const searchTextFilter =
    normalizeSearchText(debouncedSearch, 200, true) || null

  const [activeState, setActiveState] = useState<ActiveState>(
    ActiveStateValue.Active
  )

  const fetchItems = useCallback(
    async (silent = false) => {
      if (!silent) setLoading(true)
      try {
        const data = await getProphetProjects({
          activeState,
          searchText: searchTextFilter,
        })
        setItems(data)
      } catch {
        if (!silent) toast.error("Request failed. Check your connection and try again.")
      } finally {
        if (!silent) setLoading(false)
      }
    },
    [activeState, searchTextFilter]
  )

  useEffect(() => {
    void Promise.resolve().then(() => fetchItems().catch(() => {}))
  }, [fetchItems])

  const fetchItemsRef = useRef(fetchItems)
  useEffect(() => {
    fetchItemsRef.current = fetchItems
  }, [fetchItems])

  useEffect(() => {
    const hasRunning = items.some(
      (row) => row.latestPipelineStatus?.trim().toLowerCase() === "running"
    )
    if (!hasRunning) return
    const id = window.setInterval(() => {
      fetchItemsRef.current(true).catch(() => {})
    }, 4500)
    return () => window.clearInterval(id)
  }, [items])

  const handleRestore = useCallback(
    async (id: string) => {
      setRestoringId(id)
      try {
        await restoreProphetProject(id)
        await fetchItems()
        toast.success("Project restored.")
      } catch {
        toast.error("Could not restore project.")
      } finally {
        setRestoringId(null)
      }
    },
    [fetchItems]
  )

  const handleDelete = useCallback(
    async (id: string) => {
      setDeletingId(id)
      try {
        const result = await deleteProphetProject(id)
        if (result.success) {
          await fetchItems()
          toast.success("Project deleted.")
        } else {
          toast.error("Could not delete project.")
        }
      } catch {
        toast.error("Could not delete project.")
      } finally {
        setDeletingId(null)
      }
    },
    [fetchItems]
  )

  return (
    <div className="space-y-4">
      <DashboardListToolbar
        searchPlaceholder="Search project name or description..."
        searchText={searchText}
        onSearchTextChange={setSearchText}
        activeState={activeState}
        onActiveStateChange={setActiveState}
        loadingText={loading ? "Loading..." : null}
        activeStateAllLabel="All"
        activeStateActiveLabel="Active"
        activeStateInactiveLabel="Inactive"
        showActiveStateFilter
        showAddButton
        addLabel="Add"
        onAddClick={() => router.push("/prophet/new")}
      />

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Description</TableHead>
              <TableHead className="whitespace-nowrap">Expected date</TableHead>
              <TableHead className="whitespace-nowrap">Created</TableHead>
              <TableHead className="whitespace-nowrap">Pipeline</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[104px] text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.length === 0 ? (
              <TableRow>
                <TableCell
                  className="text-muted-foreground py-8 text-center"
                  colSpan={7}
                >
                  No results.
                </TableCell>
              </TableRow>
            ) : (
              items.map((row) => {
                const goEdit = () =>
                  router.push(`/prophet/edit?id=${encodeURIComponent(row.id)}`)
                const pipelineRunning =
                  row.latestPipelineStatus?.trim().toLowerCase() === "running"
                return (
                  <TableRow
                    key={row.id}
                    className={cn(
                      "cursor-pointer",
                      pipelineRunning &&
                        "bg-blue-500/[0.06] dark:bg-blue-500/10"
                    )}
                    title="Double-click to edit"
                    {...editableTableRowA11yProps(goEdit)}
                  >
                    <TableCell className="font-medium">{row.name}</TableCell>
                    <TableCell className="max-w-[360px] truncate">
                      {row.description ?? "—"}
                    </TableCell>
                    <TableCell className="text-muted-foreground whitespace-nowrap">
                      {formatExpectedDate(row.expectedDate)}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatCreatedAt(row.createdAtUtc)}
                    </TableCell>
                    <TableCell className="whitespace-nowrap">
                      <ProphetProjectListPipelineCell
                        status={row.latestPipelineStatus}
                      />
                    </TableCell>
                    <TableCell>
                      {row.isActive === false ? "Inactive" : "Active"}
                    </TableCell>
                    <TableCell
                      className="text-right"
                      onDoubleClick={stopRowDoubleClickNavigation}
                    >
                      <div className="flex items-center justify-end gap-0.5">
                        <ListRowActionTooltip label="Edit project">
                          <Button variant="ghost" size="icon-xs" asChild>
                            <Link
                              href={`/prophet/edit?id=${encodeURIComponent(row.id)}`}
                              aria-label="Edit project"
                            >
                              <PencilIcon className="size-4" />
                            </Link>
                          </Button>
                        </ListRowActionTooltip>
                        {row.isActive === false ? (
                          <ConfirmDeleteAction
                            disabled={restoringId === row.id}
                            ariaLabel="Restore project"
                            confirmTitle="Restore this project?"
                            confirmDescription="The project will be active again and visible in default lists."
                            cancelLabel="Cancel"
                            confirmActionLabel="Restore"
                            onConfirm={() => {
                              handleRestore(row.id).catch(() => {})
                            }}
                            Icon={RotateCcwIcon}
                            confirmDestructive={false}
                          />
                        ) : null}
                        {row.isActive ? (
                          <ConfirmDeleteAction
                            disabled={deletingId === row.id}
                            ariaLabel="Delete project"
                            confirmTitle="Delete this project?"
                            confirmDescription="This action cannot be undone."
                            cancelLabel="Cancel"
                            confirmActionLabel="Delete"
                            onConfirm={() => {
                              handleDelete(row.id).catch(() => {})
                            }}
                          />
                        ) : null}
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}
