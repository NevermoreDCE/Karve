import type { ThemeOptions } from "@mui/material/styles";

export const typography: ThemeOptions["typography"] = {
  fontFamily: "IBM Plex Sans, Segoe UI, sans-serif",
  h1: {
    fontFamily: "Space Grotesk, Segoe UI, sans-serif",
    fontWeight: 700,
    fontSize: "2rem",
  },
  h2: {
    fontFamily: "Space Grotesk, Segoe UI, sans-serif",
    fontWeight: 700,
    fontSize: "1.5rem",
  },
  h3: {
    fontFamily: "Space Grotesk, Segoe UI, sans-serif",
    fontWeight: 600,
    fontSize: "1.2rem",
  },
  button: {
    textTransform: "none",
    fontWeight: 600,
  },
};
