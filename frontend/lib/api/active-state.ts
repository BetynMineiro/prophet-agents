export const ActiveState = {
  All: "All",
  Active: "Active",
  Inactive: "Inactive",
} as const

export type ActiveState = (typeof ActiveState)[keyof typeof ActiveState]
