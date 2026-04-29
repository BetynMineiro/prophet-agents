/**
 * Trims and truncates search input for paged list `searchText` query params.
 * Default max length 200 matches typical API validation.
 *
 * @param lowerCase - When `true`, applies `toLowerCase()` after trim.
 */
export function normalizeSearchText(
  value: string,
  maxLen = 200,
  lowerCase = false
): string {
  const t = value.trim()
  const body = lowerCase ? t.toLowerCase() : t
  return body.slice(0, maxLen)
}

/**
 * Trim, truncate, lowercase — for paged list query params where the API
 * matches case-insensitively.
 */
export function normalizeSearchTextForPagedList(
  value: string,
  maxLen = 200
): string {
  return normalizeSearchText(value, maxLen, true)
}
