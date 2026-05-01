import { ProphetShell } from "@/components/layout/shell/prophet-shell"

export default function ProphetLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return <ProphetShell>{children}</ProphetShell>
}
