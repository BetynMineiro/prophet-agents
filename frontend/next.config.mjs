import path from "path"
import { fileURLToPath } from "url"
import createNextIntlPlugin from "next-intl/plugin"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const webRoot = __dirname

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: { unoptimized: true },
  webpack: (config) => {
    config.resolve ??= {}
    config.resolve.alias = {
      ...config.resolve.alias,
      tailwindcss: path.resolve(webRoot, "node_modules/tailwindcss"),
    }
    return config
  },
  turbopack: {
    resolveAlias: {
      tailwindcss: path.resolve(webRoot, "node_modules/tailwindcss"),
    },
  },
}

const withNextIntl = createNextIntlPlugin()
export default withNextIntl(nextConfig)
