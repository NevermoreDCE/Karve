import { useForm } from "react-hook-form";
import type { CreateProductRequest } from "../types/api";

export interface ProductFormValues extends CreateProductRequest {}

interface ProductFormProps {
  initialValues: ProductFormValues;
  onSubmit: (values: ProductFormValues) => Promise<void> | void;
  submitLabel: string;
  isSubmitting?: boolean;
  onCancel?: () => void;
}

export function ProductForm({
  initialValues,
  onSubmit,
  submitLabel,
  isSubmitting = false,
  onCancel,
}: ProductFormProps) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProductFormValues>({ defaultValues: initialValues });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <label>
        Name
        <input
          {...register("name", {
            required: "Product name is required.",
            minLength: {
              value: 1,
              message: "Product name must be between 1 and 100 characters.",
            },
            maxLength: {
              value: 100,
              message: "Product name must be between 1 and 100 characters.",
            },
          })}
        />
      </label>
      {errors.name ? <p role="alert">{errors.name.message}</p> : null}

      <label>
        SKU
        <input
          {...register("sku", {
            required: "SKU is required.",
            minLength: {
              value: 1,
              message: "SKU must be between 1 and 50 characters.",
            },
            maxLength: {
              value: 50,
              message: "SKU must be between 1 and 50 characters.",
            },
          })}
        />
      </label>
      {errors.sku ? <p role="alert">{errors.sku.message}</p> : null}

      <label>
        Unit Price
        <input
          type="number"
          step="0.01"
          min="0"
          {...register("unitPriceAmount", {
            required: "Unit price must be greater than 0.",
            valueAsNumber: true,
            validate: {
              greaterThanZero: (value) =>
                value > 0 || "Unit price must be greater than 0.",
            },
          })}
        />
      </label>
      {errors.unitPriceAmount ? <p role="alert">{errors.unitPriceAmount.message}</p> : null}

      <label>
        Currency
        <input
          maxLength={3}
          {...register("unitPriceCurrency", {
            required: "Currency is required.",
            minLength: {
              value: 3,
              message: "Currency must be exactly 3 characters.",
            },
            maxLength: {
              value: 3,
              message: "Currency must be exactly 3 characters.",
            },
            validate: {
              uppercaseIsoCode: (value) =>
                /^[A-Z]{3}$/.test(value) ||
                "Currency must be in uppercase letters (e.g., USD).",
            },
          })}
          onInput={(event) => {
            const target = event.target as HTMLInputElement;
            target.value = target.value.toUpperCase();
          }}
        />
      </label>
      {errors.unitPriceCurrency ? <p role="alert">{errors.unitPriceCurrency.message}</p> : null}

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
