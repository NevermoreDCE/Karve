import type { ReactNode } from "react";
import { Box, Toolbar } from "@mui/material";

interface AppContentProps {
  children: ReactNode;
}

const drawerWidth = 250;

export function AppContent({ children }: AppContentProps) {
  return (
    <Box
      component="main"
      sx={{
        flexGrow: 1,
        minHeight: "100vh",
        p: { xs: 2, md: 3 },
        ml: { md: `${drawerWidth}px` }, // Add left margin for permanent drawer on desktop
      }}
    >
      <Toolbar />
      {children}
    </Box>
  );
}
