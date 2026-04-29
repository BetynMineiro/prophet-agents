import { cn } from "@/lib/core/utils"

type PageHeaderProps = Readonly<{
  title: React.ReactNode
  description?: React.ReactNode
  /** Actions (buttons) aligned right on desktop. */
  actions?: React.ReactNode
  className?: string
}>

export function PageHeader({
  title,
  description,
  actions,
  className,
}: PageHeaderProps) {
  return (
    <div
      className={cn(
        "flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between sm:gap-6",
        className
      )}
    >
      <div className="min-w-0 flex-1">
        <h1 className="text-foreground text-2xl font-semibold tracking-tight sm:text-3xl">
          {title}
        </h1>
        {description && (
          <p className="text-muted-foreground mt-1.5 text-sm leading-relaxed sm:mt-2">
            {description}
          </p>
        )}
      </div>
      {actions && (
        <div className="flex shrink-0 flex-wrap items-center gap-2">
          {actions}
        </div>
      )}
    </div>
  )
}
