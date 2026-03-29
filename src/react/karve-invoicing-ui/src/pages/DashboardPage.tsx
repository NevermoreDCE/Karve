import AttachMoneyIcon from "@mui/icons-material/AttachMoney";
import HourglassTopIcon from "@mui/icons-material/HourglassTop";
import ReceiptLongIcon from "@mui/icons-material/ReceiptLong";
import TrendingUpIcon from "@mui/icons-material/TrendingUp";
import { Box, List, ListItem, ListItemText, Paper, Stack, Typography } from "@mui/material";
import { DashboardCard } from "../components/DashboardCard";

export function DashboardPage() {
  return (
    <Stack spacing={2.5}>
      <Box>
        <Typography variant="h4" component="h1">Dashboard</Typography>
        <Typography variant="body2" color="text.secondary">
          Cash flow at a glance, with alerts and invoice pipeline visibility.
        </Typography>
      </Box>

      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(2, minmax(0, 1fr))",
            lg: "repeat(4, minmax(0, 1fr))",
          },
        }}
      >
        <Box>
          <DashboardCard title="Outstanding" value="$48,720" subtitle="Across 24 invoices" icon={<AttachMoneyIcon color="primary" />} />
        </Box>
        <Box>
          <DashboardCard title="Overdue" value="$7,960" subtitle="6 invoices require action" icon={<HourglassTopIcon color="error" />} />
        </Box>
        <Box>
          <DashboardCard title="Due in 7 days" value="$12,340" subtitle="5 invoices due soon" icon={<ReceiptLongIcon color="warning" />} />
        </Box>
        <Box>
          <DashboardCard title="Received this month" value="$31,280" subtitle="+12% vs previous month" icon={<TrendingUpIcon color="success" />} />
        </Box>
      </Box>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="h6" sx={{ mb: 1 }}>Recent Activity</Typography>
        <List dense disablePadding>
          <ListItem disableGutters><ListItemText primary="Invoice #1042 sent" secondary="2 hours ago" /></ListItem>
          <ListItem disableGutters><ListItemText primary="Payment received from Contoso" secondary="5 hours ago" /></ListItem>
          <ListItem disableGutters><ListItemText primary="Customer Fabrikam added" secondary="Yesterday" /></ListItem>
        </List>
      </Paper>
    </Stack>
  );
}
