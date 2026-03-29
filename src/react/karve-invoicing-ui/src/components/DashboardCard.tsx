import type { ReactNode } from "react";
import { Card, CardContent, Stack, Typography } from "@mui/material";

interface DashboardCardProps {
  title: string;
  value: string;
  subtitle?: string;
  icon?: ReactNode;
}

export function DashboardCard({ title, value, subtitle, icon }: DashboardCardProps) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={1}>
          <Stack spacing={0.5}>
            <Typography variant="body2" color="text.secondary">{title}</Typography>
            <Typography variant="h5">{value}</Typography>
            {subtitle ? <Typography variant="caption" color="text.secondary">{subtitle}</Typography> : null}
          </Stack>
          {icon}
        </Stack>
      </CardContent>
    </Card>
  );
}
