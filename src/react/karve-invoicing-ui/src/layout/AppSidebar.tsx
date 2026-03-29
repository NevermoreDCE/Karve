import DashboardIcon from "@mui/icons-material/Dashboard";
import DescriptionIcon from "@mui/icons-material/Description";
import GroupsIcon from "@mui/icons-material/Groups";
import Inventory2Icon from "@mui/icons-material/Inventory2";
import { Box, Divider, Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar } from "@mui/material";
import { NavLink } from "react-router-dom";

const drawerWidth = 250;

const links = [
  { to: "/", label: "Dashboard", icon: <DashboardIcon fontSize="small" /> },
  { to: "/invoices", label: "Invoices", icon: <DescriptionIcon fontSize="small" /> },
  { to: "/customers", label: "Customers", icon: <GroupsIcon fontSize="small" /> },
  { to: "/products", label: "Products", icon: <Inventory2Icon fontSize="small" /> },
];

interface AppSidebarProps {
  mobileOpen: boolean;
  onClose: () => void;
}

export function AppSidebar({ mobileOpen, onClose }: AppSidebarProps) {
  const drawerContent = (
    <Box>
      <Toolbar />
      <Divider />
      <List>
        {links.map((link) => (
          <ListItemButton
            key={link.to}
            component={NavLink}
            to={link.to}
            end={link.to === "/"}
            onClick={onClose}
            sx={{
              "&.active": {
                bgcolor: "action.selected",
                borderRight: 3,
                borderColor: "primary.main",
              },
            }}
          >
            <ListItemIcon>{link.icon}</ListItemIcon>
            <ListItemText primary={link.label} />
          </ListItemButton>
        ))}
      </List>
    </Box>
  );

  return (
    <>
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: "block", md: "none" },
          "& .MuiDrawer-paper": { width: drawerWidth, boxSizing: "border-box" },
        }}
      >
        {drawerContent}
      </Drawer>

      <Drawer
        variant="permanent"
        open
        sx={{
          display: { xs: "none", md: "block" },
          "& .MuiDrawer-paper": { width: drawerWidth, boxSizing: "border-box" },
        }}
      >
        {drawerContent}
      </Drawer>
    </>
  );
}
