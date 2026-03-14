import { useForm } from "react-hook-form";
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
      <label>
        Name
        <input
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
        />
      </label>
      {errors.name ? <p role="alert">{errors.name.message}</p> : null}

      <label>
        Email
        <input
          type="email"
          {...register("email", {
            required: "Email is required.",
            pattern: {
              value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
              message: "A valid email address is required.",
            },
          })}
        />
      </label>
      {errors.email ? <p role="alert">{errors.email.message}</p> : null}

      <label>
        Billing Address
        <input
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
        />
      </label>
      {errors.billingAddress ? <p role="alert">{errors.billingAddress.message}</p> : null}

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
