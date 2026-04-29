import type { ComponentProps } from "react"

/** `onSubmit` do `<form>` — evita importar `FormEvent` de `react` (obsoleto em Sonar). */
export type FormSubmitEvent = Parameters<
  NonNullable<ComponentProps<"form">["onSubmit"]>
>[0]
