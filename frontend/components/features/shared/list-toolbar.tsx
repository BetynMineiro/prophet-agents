"use client"

import { Input } from "@/components/ui/input/input"
import { Button } from "@/components/ui/button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select/select"
import type { ActiveState } from "@/lib/api/active-state"

type Props = Readonly<{
  searchPlaceholder: string
  searchText: string
  onSearchTextChange: (value: string) => void
  activeState: ActiveState
  onActiveStateChange: (value: ActiveState) => void
  loadingText: string | null
  activeStateAllLabel: string
  activeStateActiveLabel: string
  activeStateInactiveLabel: string
  showAddButton?: boolean
  addLabel?: string
  onAddClick?: () => void
  addDisabled?: boolean
  /** When false, only search + add. Default true. */
  showActiveStateFilter?: boolean
}>

export function DashboardListToolbar({
  searchPlaceholder,
  searchText,
  onSearchTextChange,
  activeState,
  onActiveStateChange,
  loadingText,
  activeStateAllLabel,
  activeStateActiveLabel,
  activeStateInactiveLabel,
  showAddButton = false,
  addLabel,
  onAddClick,
  addDisabled = false,
  showActiveStateFilter = true,
}: Props) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex min-w-0 flex-1 flex-col gap-3 sm:flex-row sm:items-center">
        <Input
          className="min-w-0 flex-1"
          placeholder={searchPlaceholder}
          value={searchText}
          onChange={(e) => onSearchTextChange(e.target.value)}
        />
        {showActiveStateFilter ? (
          <Select
            value={activeState}
            onValueChange={(v) => onActiveStateChange(v as ActiveState)}
          >
            <SelectTrigger size="sm" className="w-full sm:w-[200px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Active">{activeStateActiveLabel}</SelectItem>
              <SelectItem value="Inactive">
                {activeStateInactiveLabel}
              </SelectItem>
              <SelectItem value="All">{activeStateAllLabel}</SelectItem>
            </SelectContent>
          </Select>
        ) : null}
      </div>
      <div className="flex shrink-0 items-center gap-3">
        <div className="text-muted-foreground text-sm">{loadingText}</div>
        {showAddButton && addLabel && onAddClick ? (
          <Button
            size="sm"
            className="self-start sm:self-center"
            onClick={onAddClick}
            disabled={addDisabled}
          >
            {addLabel}
          </Button>
        ) : null}
      </div>
    </div>
  )
}
