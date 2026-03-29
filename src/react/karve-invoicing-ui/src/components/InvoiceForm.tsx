import { useMemo } from "react";
import dayjs from "dayjs";
import { Controller, useForm } from "react-hook-form";
import {
  Alert,
  Button,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers";
import type { InvoiceStatus } from "../types/api";

const invoiceStatuses: InvoiceStatus[] = [
  "Draft",
  "Sent",
  "Viewed",
  "Paid",
  "Overdue",
  "Canceled",
];

export interface InvoiceFormValues {
  customerId: string;
  invoiceDate: string;
  dueDate: string;
  status: InvoiceStatus;
}

interface InvoiceFormProps {
  initialValues: InvoiceFormValues;
  onSubmit: (values: InvoiceFormValues) => Promise<void> | void;
  submitLabel: string;
  isSubmitting?: boolean;
  onCancel?: () => void;
}

export function InvoiceForm({
  initialValues,
  onSubmit,
  submitLabel,
  isSubmitting = false,
  onCancel,
}: InvoiceFormProps) {
  const {
    register,
    handleSubmit,
    watch,
    reset,
    control,
    formState: { errors },
  } = useForm<InvoiceFormValues>({ defaultValues: initialValues });

  const invoiceDateValue = watch("invoiceDate");
  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <TextField
          label="Customer ID"
          {...register("customerId", {
            required: "Customer ID is required.",
          })}
          error={!!errors.customerId}
          helperText={errors.customerId?.message}
        />

        <Controller
          control={control}
          name="invoiceDate"
          rules={{
            required: "Invoice date is required.",
            validate: {
              notInFuture: (value) => value <= today || "Invoice date cannot be in the future.",
            },
          }}
          render={({ field }) => (
            <DatePicker
              label="Invoice Date"
              value={field.value ? dayjs(field.value) : null}
              onChange={(value) => field.onChange(value ? value.format("YYYY-MM-DD") : "")}
              slotProps={{
                textField: {
                  error: !!errors.invoiceDate,
                  helperText: errors.invoiceDate?.message,
                },
              }}
            />
          )}
        />

        <Controller
          control={control}
          name="dueDate"
          rules={{
            required: "Due date is required.",
            validate: {
              afterInvoiceDate: (value) => value > invoiceDateValue || "Due date must be after the invoice date.",
            },
          }}
          render={({ field }) => (
            <DatePicker
              label="Due Date"
              value={field.value ? dayjs(field.value) : null}
              onChange={(value) => field.onChange(value ? value.format("YYYY-MM-DD") : "")}
              slotProps={{
                textField: {
                  error: !!errors.dueDate,
                  helperText: errors.dueDate?.message,
                },
              }}
            />
          )}
        />

        <TextField
          select
          label="Status"
          defaultValue={initialValues.status}
          {...register("status", {
            required: "Invalid invoice status.",
            validate: {
              inEnum: (value) => invoiceStatuses.includes(value) || "Invalid invoice status.",
            },
          })}
          error={!!errors.status}
          helperText={errors.status?.message}
        >
          {invoiceStatuses.map((status) => (
            <MenuItem key={status} value={status}>{status}</MenuItem>
          ))}
        </TextField>

        {(errors.invoiceDate || errors.dueDate || errors.status || errors.customerId) ? (
          <Alert severity="warning">Please correct the highlighted fields.</Alert>
        ) : null}

        <Stack direction="row" spacing={1} justifyContent="flex-end">
          {onCancel ? (
            <Button
              type="button"
              variant="outlined"
              onClick={() => {
                reset(initialValues);
                onCancel();
              }}
            >
              Cancel
            </Button>
          ) : null}
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : submitLabel}
          </Button>
        </Stack>
      </Stack>
    </form>
  );
}
