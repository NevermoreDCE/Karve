import type { PaletteOptions } from "@mui/material/styles";

export const lightPalette: PaletteOptions = {
  mode: "light",
  primary: { main: "rgba(70,130,180,1)" },
  secondary: { main: "rgba(176,196,222,1)" },
  success: { main: "rgba(60,179,113,1)" },
  error: { main: "rgba(220,20,60,1)" },
  background: {
    default: "rgba(245,245,245,1)",
    paper: "rgba(255,255,255,1)",
  },
  text: {
    primary: "rgba(33,33,33,1)",
    secondary: "rgba(112,128,144,1)",
  },
  divider: "rgba(192,192,192,0.55)",
};

export const darkPalette: PaletteOptions = {
  mode: "dark",
  primary: { main: "rgba(0,105,92,1)" },
  secondary: { main: "rgba(192,192,192,1)" },
  success: { main: "rgba(60,179,113,1)" },
  error: { main: "rgba(220,20,60,1)" },
  background: {
    default: "rgba(18,18,18,1)",
    paper: "rgba(38,50,56,1)",
  },
  text: {
    primary: "rgba(230,230,230,1)",
    secondary: "rgba(192,192,192,1)",
  },
  divider: "rgba(192,192,192,0.3)",
};
