"use client"

import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import { useLocale, useTranslations } from "next-intl"
import {
  deleteProphetProject,
  getProphetProjectsPage,
  restoreProphetProject,
  type ProphetProjectItemDto,
} from "@/lib/api/prophet"
import { normalizeSearchText } from "@/lib/api/normalize-search-text"
import {
  ActiveState as ActiveStateValue,
  type ActiveState,
} from "@/lib/api/active-state"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import { Link, useRouter } from "@/i18n/navigation"
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
import { DashboardListPagination } from "../shared/list-pagination"
import { DashboardListToolbar } from "../shared/list-toolbar"
import { ConfirmDeleteAction } from "../shared/confirm-delete-action"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { ProphetProjectListPipelineCell } from "@/components/features/prophet/prophet-project-list-pipeline-cell"
import { cn } from "@/lib/core/utils"

function formatCreatedAt(iso: string, locale: string): string {
  try {
    return new Date(iso).toLocaleString(locale, {
      dateStyle: "short",
      timeStyle: "short",
    })
  } catch {
    return iso
  }
}

/** `isoDate` is `YYYY-MM-DD` from the API */
function formatExpectedDate(isoDate: string | null, locale: string): string {
  if (!isoDate) return "—"
  try {
    const [y, m, d] = isoDate.split("-").map(Number)
    if (!y || !m || !d) return isoDate
    return new Date(y, m - 1, d).toLocaleDateString(locale, {
      dateStyle: "short",
    })
  } catch {
    return isoDate
  }
}

export function ProphetProjectsList() {
  const t = useTranslations("dashboard")
  const router = useRouter()
  const locale = useLocale()
  const pageSize = 10
  const [cursor, setCursor] = useState<string | null>(null)
  const [items, setItems] = useState<ProphetProjectItemDto[]>([])
  const [hasNext, setHasNext] = useState(false)
  const [loading, setLoading] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [restoringId, setRestoringId] = useState<string | null>(null)

  const [searchText, setSearchText] = useState("")
  const debouncedSearch = useDebouncedValue(searchText, 400)
  const effectiveSearch = useMemo(
    () => normalizeSearchText(debouncedSearch, 200, true),
    [debouncedSearch]
  )
  const searchTextFilter = effectiveSearch || null

  const [activeState, setActiveState] = useState<ActiveState>(
    ActiveStateValue.Active
  )

  const fetchPage = useCallback(
    async (opts: {
      cursor: string | null
      resetItems: boolean
      /** Background refresh (e.g. pipeline polling): do not toggle list loading or pagination spinner. */
      silent?: boolean
    }) => {
      const silent = opts.silent === true
      if (!silent) setLoading(true)
      try {
        const page = await getProphetProjectsPage({
          activeState,
          pageSize,
          cursor: opts.cursor,
          searchText: searchTextFilter,
        })
        setHasNext(page.hasNext)
        setItems((prev) =>
          opts.resetItems ? page.items : [...prev, ...page.items]
        )
        setCursor(page.nextCursor)
      } catch {
        if (!silent) toast.error(t("requestFailed"))
      } finally {
        if (!silent) setLoading(false)
      }
    },
    [activeState, pageSize, searchTextFilter, t]
  )

  useEffect(() => {
    void Promise.resolve().then(() =>
      fetchPage({ cursor: null, resetItems: true }).catch(() => {})
    )
  }, [fetchPage])

  const fetchPageRef = useRef(fetchPage)
  useEffect(() => {
    fetchPageRef.current = fetchPage
  }, [fetchPage])

  /** While at least one visible row is Running, refresh the list so the correct line keeps the spinner. */
  useEffect(() => {
    const hasRunning = items.some(
      (row) => row.latestPipelineStatus?.trim().toLowerCase() === "running"
    )
    if (!hasRunning) return
    const id = window.setInterval(() => {
      fetchPageRef
        .current({
          cursor: null,
          resetItems: true,
          silent: true,
        })
        .catch(() => {})
    }, 4500)
    return () => window.clearInterval(id)
  }, [items])

  const handleRestore = useCallback(
    async (id: string) => {
      setRestoringId(id)
      try {
        await restoreProphetProject(id)
        await fetchPage({ cursor: null, resetItems: true })
        toast.success(t("prophetRestoreSuccess"))
      } catch {
        toast.error(t("prophetRestoreFailed"))
      } finally {
        setRestoringId(null)
      }
    },
    [fetchPage, t]
  )

  const handleDelete = useCallback(
    async (id: string) => {
      setDeletingId(id)
      try {
        const result = await deleteProphetProject(id)
        if (result.success) {
          await fetchPage({ cursor: null, resetItems: true })
          toast.success(t("prophetDeleteSuccess"))
        } else {
          toast.error(t("prophetDeleteFailed"))
        }
      } catch {
        toast.error(t("prophetDeleteFailed"))
      } finally {
        setDeletingId(null)
      }
    },
    [fetchPage, t]
  )

  return (
    <div className="space-y-4">
      <DashboardListToolbar
        searchPlaceholder={t("prophetSearchPlaceholder")}
        searchText={searchText}
        onSearchTextChange={setSearchText}
        activeState={activeState}
        onActiveStateChange={setActiveState}
        loadingText={loading ? t("tableLoading") : null}
        activeStateAllLabel={t("activeStateFilterAll")}
        activeStateActiveLabel={t("activeStateFilterActive")}
        activeStateInactiveLabel={t("activeStateFilterInactive")}
        showActiveStateFilter
        showAddButton
        addLabel={t("addActionLabel")}
        onAddClick={() => router.push("/prophet/new")}
      />

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("prophetTableName")}</TableHead>
              <TableHead>{t("prophetTableDescription")}</TableHead>
              <TableHead className="whitespace-nowrap">
                {t("prophetTableExpectedDate")}
              </TableHead>
              <TableHead className="whitespace-nowrap">
                {t("prophetTableCreated")}
              </TableHead>
              <TableHead className="whitespace-nowrap">
                {t("prophetTablePipeline")}
              </TableHead>
              <TableHead>{t("prophetTableStatus")}</TableHead>
              <TableHead className="w-[104px] text-right">
                {t("prophetTableActions")}
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.length === 0 ? (
              <TableRow>
                <TableCell
                  className="text-muted-foreground py-8 text-center"
                  colSpan={7}
                >
                  {t("tableNoResults")}
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
                    title={t("tableRowDoubleClickToEdit")}
                    {...editableTableRowA11yProps(goEdit)}
                  >
                    <TableCell className="font-medium">{row.name}</TableCell>
                    <TableCell className="max-w-[360px] truncate">
                      {row.description ?? "—"}
                    </TableCell>
                    <TableCell className="text-muted-foreground whitespace-nowrap">
                      {formatExpectedDate(row.expectedDate, locale)}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatCreatedAt(row.createdAtUtc, locale)}
                    </TableCell>
                    <TableCell className="whitespace-nowrap">
                      <ProphetProjectListPipelineCell
                        status={row.latestPipelineStatus}
                        t={t}
                      />
                    </TableCell>
                    <TableCell>
                      {row.isActive === false
                        ? t("statusInactive")
                        : t("statusActive")}
                    </TableCell>
                    <TableCell
                      className="text-right"
                      onDoubleClick={stopRowDoubleClickNavigation}
                    >
                      <div className="flex items-center justify-end gap-0.5">
                        <ListRowActionTooltip
                          label={t("prophetEditActionAriaLabel")}
                        >
                          <Button variant="ghost" size="icon-xs" asChild>
                            <Link
                              href={`/prophet/edit?id=${encodeURIComponent(row.id)}`}
                              aria-label={t("prophetEditActionAriaLabel")}
                            >
                              <PencilIcon className="size-4" />
                            </Link>
                          </Button>
                        </ListRowActionTooltip>
                        {row.isActive === false ? (
                          <ConfirmDeleteAction
                            disabled={restoringId === row.id}
                            ariaLabel={t("prophetRestoreActionAriaLabel")}
                            confirmTitle={t("prophetRestoreConfirmTitle")}
                            confirmDescription={t(
                              "prophetRestoreConfirmDescription"
                            )}
                            cancelLabel={t("prophetRestoreCancelLabel")}
                            confirmActionLabel={t(
                              "prophetRestoreConfirmAction"
                            )}
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
                            ariaLabel={t("prophetDeleteActionAriaLabel")}
                            confirmTitle={t("prophetDeleteConfirmTitle")}
                            confirmDescription={t(
                              "prophetDeleteConfirmDescription"
                            )}
                            cancelLabel={t("prophetDeleteCancelLabel")}
                            confirmActionLabel={t("prophetDeleteConfirmAction")}
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

      <DashboardListPagination
        hasNext={hasNext}
        loading={loading}
        cursor={cursor}
        onNext={(nextCursor) => {
          fetchPage({ cursor: nextCursor, resetItems: false }).catch(() => {})
        }}
        previousLabel={t("paginationPreviousLabel")}
        nextLabel={t("paginationNextLabel")}
        moreResultsLabel={t("paginationMoreResultsAvailable")}
        endOfResultsLabel={t("paginationEndOfResults")}
      />
    </div>
  )
}
