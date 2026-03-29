import type { Components, Theme } from "@mui/material/styles";

export const components: Components<Theme> = {
  MuiButton: {
    defaultProps: {
      variant: "contained",
      size: "medium",
    },
    styleOverrides: {
      root: {
        borderRadius: 10,
      },
    },
  },
  MuiTextField: {
    defaultProps: {
      fullWidth: true,
      size: "small",
    },
  },
  MuiDialog: {
    defaultProps: {
      fullWidth: true,
      maxWidth: "sm",
    },
    styleOverrides: {
      paper: {
        borderRadius: 14,
      },
    },
  },
  MuiTableCell: {
    styleOverrides: {
      head: {
        fontWeight: 700,
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 14,
      },
    },
  },
};
