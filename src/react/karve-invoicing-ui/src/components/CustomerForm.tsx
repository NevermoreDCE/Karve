import { useForm } from "react-hook-form";
import { Button, Stack, TextField } from "@mui/material";
import type { CreateCustomerRequest } from "../types/api";

export interface CustomerFormValues extends CreateCustomerRequest {}

interface CustomerFormProps {
  initialValues: CustomerFormValues;
  onSubmit: (values: CustomerFormValues) => Promise<void> | void;
  submitLabel: string;
  isSubmitting?: boolean;
  onCancel?: () => void;
}

export function CustomerForm({
  initialValues,
  onSubmit,
  submitLabel,
  isSubmitting = false,
  onCancel,
}: CustomerFormProps) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CustomerFormValues>({ defaultValues: initialValues });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <TextField
          label="Name"
          {...register("name", {
            required: "Customer name is required.",
            minLength: {
              value: 1,
              message: "Customer name must be between 1 and 100 characters.",
            },
            maxLength: {
              value: 100,
              message: "Customer name must be between 1 and 100 characters.",
            },
          })}
          error={!!errors.name}
          helperText={errors.name?.message}
        />

        <TextField
          label="Email"
          type="email"
          {...register("email", {
            required: "Email is required.",
            pattern: {
              value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
              message: "A valid email address is required.",
            },
          })}
          error={!!errors.email}
          helperText={errors.email?.message}
        />

        <TextField
          label="Billing Address"
          multiline
          minRows={2}
          {...register("billingAddress", {
            required: "Billing address is required.",
            minLength: {
              value: 1,
              message: "Billing address must be between 1 and 500 characters.",
            },
            maxLength: {
              value: 500,
              message: "Billing address must be between 1 and 500 characters.",
            },
          })}
          error={!!errors.billingAddress}
          helperText={errors.billingAddress?.message}
        />

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
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : submitLabel}
          </Button>
        </Stack>
      </Stack>
    </form>
  );
}
