"use client"

import { Children, isValidElement, useLayoutEffect, useRef } from "react"
import ReactMarkdown, { type Components } from "react-markdown"
import remarkGfm from "remark-gfm"

function MermaidDiagram({ chart }: Readonly<{ chart: string }>) {
  const hostRef = useRef<HTMLDivElement>(null)

  useLayoutEffect(() => {
    const host = hostRef.current
    if (!host) return
    let cancelled = false
    host.replaceChildren()

    void import("mermaid").then((mermaidMod) => {
      if (cancelled || !hostRef.current) return
      const mermaid = mermaidMod.default
      const isDark = document.documentElement.classList.contains("dark")
      mermaid.initialize({
        startOnLoad: false,
        securityLevel: "loose",
        theme: isDark ? "dark" : "default",
      })
      const el = document.createElement("pre")
      el.className = "mermaid"
      el.textContent = chart
      host.appendChild(el)
      void mermaid.run({ nodes: [el] })
    })

    return () => {
      cancelled = true
    }
  }, [chart])

  return (
    <div
      ref={hostRef}
      className="bg-muted/30 my-4 flex min-h-[4rem] w-full justify-center overflow-x-auto rounded-md border p-4"
    />
  )
}

function buildComponents(): Components {
  return {
    pre({ children }) {
      const arr = Children.toArray(children)
      const codeEl = arr.find(
        (c) =>
          isValidElement(c) &&
          typeof c.props === "object" &&
          c.props !== null &&
          "className" in c.props &&
          typeof (c.props as { className?: string }).className === "string" &&
          ((c.props as { className?: string }).className?.includes(
            "language-mermaid"
          ) ??
            false)
      )
      if (isValidElement(codeEl)) {
        const text = String(
          (codeEl.props as { children?: React.ReactNode }).children ?? ""
        ).replace(/\n$/, "")
        return <MermaidDiagram chart={text} />
      }
      return (
        <pre className="bg-muted/50 my-2 overflow-x-auto rounded-md border p-3 text-xs">
          {children}
        </pre>
      )
    },
    code({ className, children, ...props }) {
      const inline = !className
      if (inline) {
        return (
          <code
            className="bg-muted rounded px-1 py-0.5 font-mono text-[0.9em]"
            {...props}
          >
            {children}
          </code>
        )
      }
      return (
        <code className={className} {...props}>
          {children}
        </code>
      )
    },
    h1: ({ children }) => (
      <h1 className="mt-4 mb-2 text-xl font-semibold first:mt-0">{children}</h1>
    ),
    h2: ({ children }) => (
      <h2 className="mt-4 mb-2 text-lg font-semibold first:mt-0">{children}</h2>
    ),
    h3: ({ children }) => (
      <h3 className="mt-3 mb-1 text-base font-semibold first:mt-0">
        {children}
      </h3>
    ),
    p: ({ children }) => <p className="my-2 leading-relaxed">{children}</p>,
    ul: ({ children }) => (
      <ul className="my-2 list-inside list-disc space-y-1">{children}</ul>
    ),
    ol: ({ children }) => (
      <ol className="my-2 list-inside list-decimal space-y-1">{children}</ol>
    ),
    li: ({ children }) => <li className="leading-relaxed">{children}</li>,
    blockquote: ({ children }) => (
      <blockquote className="border-muted-foreground/40 my-2 border-l-4 pl-4 italic">
        {children}
      </blockquote>
    ),
    a: ({ children, href }) => (
      <a
        href={href}
        className="text-primary underline underline-offset-2"
        target="_blank"
        rel="noopener noreferrer"
      >
        {children}
      </a>
    ),
    table: ({ children }) => (
      <div className="my-2 overflow-x-auto">
        <table className="w-full border-collapse text-sm">{children}</table>
      </div>
    ),
    th: ({ children }) => (
      <th className="border-border bg-muted/50 border px-2 py-1 text-start font-medium">
        {children}
      </th>
    ),
    td: ({ children }) => (
      <td className="border-border border px-2 py-1 align-top">{children}</td>
    ),
  }
}

export function MermaidMarkdown({
  markdown,
  className,
}: Readonly<{ markdown: string; className?: string }>) {
  return (
    <div className={className}>
      <ReactMarkdown remarkPlugins={[remarkGfm]} components={buildComponents()}>
        {markdown}
      </ReactMarkdown>
    </div>
  )
}
