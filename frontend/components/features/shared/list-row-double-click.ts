import type { KeyboardEvent, MouseEvent } from "react"

/** Prevents double-click on the actions column from triggering row-level navigation. */
export function stopRowDoubleClickNavigation(
  e: MouseEvent<HTMLTableCellElement | HTMLDivElement>
): void {
  e.stopPropagation()
}

/** Enter / Space on a focused row navigates to edit (parity with double-click). */
export function rowNavigateEditKeyDown(
  e: KeyboardEvent<HTMLTableRowElement>,
  onNavigate: () => void
): void {
  if (e.key !== "Enter" && e.key !== " ") return
  e.preventDefault()
  onNavigate()
}

/**
 * Spread onto `<TableRow>` so double-click and keyboard (Enter/Space) navigate;
 * adds role/tabIndex because native `<tr>` is not focusable by default.
 */
export function editableTableRowA11yProps(onNavigate: () => void) {
  return {
    role: "link" as const,
    tabIndex: 0 as const,
    onKeyDown: (e: KeyboardEvent<HTMLTableRowElement>) =>
      rowNavigateEditKeyDown(e, onNavigate),
    onDoubleClick: onNavigate,
  }
}
