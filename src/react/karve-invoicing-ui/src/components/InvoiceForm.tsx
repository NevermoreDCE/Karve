import { useMemo } from "react";
import { useForm } from "react-hook-form";
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
    formState: { errors },
  } = useForm<InvoiceFormValues>({ defaultValues: initialValues });

  const invoiceDateValue = watch("invoiceDate");
  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <label>
        Customer ID
        <input
          {...register("customerId", {
            required: "Customer ID is required.",
          })}
        />
      </label>
      {errors.customerId ? <p role="alert">{errors.customerId.message}</p> : null}

      <label>
        Invoice Date
        <input
          type="date"
          {...register("invoiceDate", {
            required: "Invoice date is required.",
            validate: {
              notInFuture: (value) =>
                value <= today || "Invoice date cannot be in the future.",
            },
          })}
        />
      </label>
      {errors.invoiceDate ? <p role="alert">{errors.invoiceDate.message}</p> : null}

      <label>
        Due Date
        <input
          type="date"
          {...register("dueDate", {
            required: "Due date is required.",
            validate: {
              afterInvoiceDate: (value) =>
                value > invoiceDateValue || "Due date must be after the invoice date.",
            },
          })}
        />
      </label>
      {errors.dueDate ? <p role="alert">{errors.dueDate.message}</p> : null}

      <label>
        Status
        <select
          {...register("status", {
            required: "Invalid invoice status.",
            validate: {
              inEnum: (value) =>
                invoiceStatuses.includes(value) || "Invalid invoice status.",
            },
          })}
        >
          {invoiceStatuses.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>
      </label>
      {errors.status ? <p role="alert">{errors.status.message}</p> : null}

      <div style={{ marginTop: 8 }}>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : submitLabel}
        </button>
        {onCancel ? (
          <button
            type="button"
            style={{ marginLeft: 8 }}
            onClick={() => {
              reset(initialValues);
              onCancel();
            }}
          >
            Cancel
          </button>
        ) : null}
      </div>
    </form>
  );
}
