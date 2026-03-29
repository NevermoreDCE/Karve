import { createTheme } from "@mui/material/styles";
import { components } from "./components";
import { darkPalette, lightPalette } from "./palette";
import { typography } from "./typography";

export type ThemeMode = "light" | "dark";

export function createAppTheme(mode: ThemeMode) {
  return createTheme({
    palette: mode === "dark" ? darkPalette : lightPalette,
    typography,
    shape: {
      borderRadius: 10,
    },
    components,
  });
}
