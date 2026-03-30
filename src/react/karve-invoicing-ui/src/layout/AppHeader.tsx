import MenuIcon from "@mui/icons-material/Menu";
import DarkModeIcon from "@mui/icons-material/DarkMode";
import LightModeIcon from "@mui/icons-material/LightMode";
import SettingsIcon from "@mui/icons-material/Settings";
import {
  AppBar,
  Box,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  Toolbar,
  Tooltip,
  Typography,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from "@mui/material";
import { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import { useCurrentUser } from "../hooks/useCurrentUser";
import { useThemeStore } from "../state/themeStore";

interface AppHeaderProps {
  onMenuClick: () => void;
}

export function AppHeader({ onMenuClick }: AppHeaderProps) {
  const { isAuthenticated, login, logout } = useAuth();
  const { profile, memberships, selectedCompanyId, setSelectedCompanyId, isLoading } = useCurrentUser();
  const { themeMode, toggleTheme } = useThemeStore();
  const [openPreferences, setOpenPreferences] = useState(false);

  const displayName = profile.displayName ?? profile.email ?? "";
  const selectedCompany = memberships.find((c) => c.id === selectedCompanyId);
  const companyName = selectedCompany?.name ?? "Company";

  return (
    <>
      <AppBar position="fixed" color="default" elevation={0} sx={{ borderBottom: 1, borderColor: "divider", zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar sx={{ gap: 1.5 }}>
          <IconButton edge="start" color="inherit" onClick={onMenuClick} sx={{ display: { md: "none" } }}>
            <MenuIcon />
          </IconButton>

          <Typography
            sx={{
              fontFamily: "'Uncial Antiqua', serif",
              fontSize: "2.5rem",
              fontWeight: 700,
              mr: 2,
            }}
          >
            Karve
          </Typography>

          <Box sx={{ minWidth: 220 }}>
            <Typography variant="h6" component="h1">{companyName}</Typography>
            {isAuthenticated && displayName ? (
              <Typography variant="caption" color="text.secondary">{displayName}</Typography>
            ) : null}
          </Box>

          <Box sx={{ flexGrow: 1 }} />

          <Stack direction="row" alignItems="center" spacing={1.5}>
            {isAuthenticated && memberships.length > 1 ? (
              <FormControl size="small" sx={{ minWidth: 220 }}>
                <InputLabel id="company-select-label">Company</InputLabel>
                <Select
                  labelId="company-select-label"
                  label="Company"
                  value={selectedCompanyId ?? ""}
                  onChange={(event) => setSelectedCompanyId(event.target.value)}
                  disabled={isLoading}
                >
                  {memberships.map((company) => (
                    <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            ) : null}

            {isAuthenticated ? (
              <Tooltip title="User preferences">
                <IconButton color="inherit" onClick={() => setOpenPreferences(true)}>
                  <SettingsIcon />
                </IconButton>
              </Tooltip>
            ) : null}

            {isAuthenticated ? (
              <Button variant="outlined" color="inherit" onClick={logout}>Sign out</Button>
            ) : (
              <Button variant="contained" onClick={login}>Sign in</Button>
            )}
          </Stack>
        </Toolbar>
      </AppBar>

      <Dialog open={openPreferences} onClose={() => setOpenPreferences(false)}>
        <DialogTitle>User Preferences</DialogTitle>
        <DialogContent>
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mt: 1 }}>
            <Stack direction="row" spacing={1} alignItems="center">
              {themeMode === "dark" ? <DarkModeIcon fontSize="small" /> : <LightModeIcon fontSize="small" />}
              <Typography>Dark mode</Typography>
            </Stack>
            <Switch checked={themeMode === "dark"} onChange={toggleTheme} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenPreferences(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
