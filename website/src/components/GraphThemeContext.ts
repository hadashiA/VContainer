import { useColorMode, ColorMode } from "@docusaurus/theme-common"
import { Theme } from "@nivo/core"

const textColros: Record<ColorMode, string> = {
  light: "rgb(28, 30, 33)",
  dark: "rgb(245, 246, 247)"
} as const

type GraphThemeContext = {
  theme: Theme
}

export function useGraphTheme(): GraphThemeContext {
  const { colorMode } = useColorMode()
  return {
    theme: { textColor: textColros[colorMode] }
  }
}
