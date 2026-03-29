import type { ReactElement } from "react";
import CancelOutlinedIcon from "@mui/icons-material/CancelOutlined";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import RemoveRedEyeOutlinedIcon from "@mui/icons-material/RemoveRedEyeOutlined";
import SendOutlinedIcon from "@mui/icons-material/SendOutlined";
import RadioButtonUncheckedIcon from "@mui/icons-material/RadioButtonUnchecked";
import { Chip } from "@mui/material";
import type { InvoiceStatus } from "../types/api";

const icons: Record<InvoiceStatus, ReactElement> = {
  Draft: <RadioButtonUncheckedIcon fontSize="small" />,
  Sent: <SendOutlinedIcon fontSize="small" />,
  Viewed: <RemoveRedEyeOutlinedIcon fontSize="small" />,
  Paid: <CheckCircleOutlineIcon fontSize="small" />,
  Overdue: <ErrorOutlineIcon fontSize="small" />,
  Canceled: <CancelOutlinedIcon fontSize="small" />,
};

const chipColor: Record<InvoiceStatus, "default" | "primary" | "success" | "error" | "warning"> = {
  Draft: "default",
  Sent: "primary",
  Viewed: "warning",
  Paid: "success",
  Overdue: "error",
  Canceled: "default",
};

interface StatusBadgeProps {
  status: InvoiceStatus;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <Chip size="small" variant="outlined" icon={icons[status]} color={chipColor[status]} label={status} />
  );
}
