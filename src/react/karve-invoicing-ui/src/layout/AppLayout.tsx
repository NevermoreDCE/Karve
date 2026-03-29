import type { ReactNode } from "react";
import { useState } from "react";
import { Box } from "@mui/material";
import { AppContent } from "./AppContent";
import { AppHeader } from "./AppHeader";
import { AppSidebar } from "./AppSidebar";

interface AppLayoutProps {
  children: ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <AppHeader onMenuClick={() => setMobileOpen((prev) => !prev)} />
      <AppSidebar mobileOpen={mobileOpen} onClose={() => setMobileOpen(false)} />
      <AppContent>{children}</AppContent>
    </Box>
  );
}
