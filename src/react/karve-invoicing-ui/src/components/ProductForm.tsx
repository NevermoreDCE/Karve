import { useForm } from "react-hook-form";
import { Button, MenuItem, Stack, TextField } from "@mui/material";
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
      <Stack spacing={2}>
        <TextField
          label="Name"
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
          error={!!errors.name}
          helperText={errors.name?.message}
        />

        <TextField
          label="SKU"
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
          error={!!errors.sku}
          helperText={errors.sku?.message}
        />

        <TextField
          label="Unit Price"
          type="number"
          inputProps={{ step: "0.01", min: 0 }}
          {...register("unitPriceAmount", {
            required: "Unit price must be greater than 0.",
            valueAsNumber: true,
            validate: {
              greaterThanZero: (value) => value > 0 || "Unit price must be greater than 0.",
            },
          })}
          error={!!errors.unitPriceAmount}
          helperText={errors.unitPriceAmount?.message}
        />

        <TextField
          select
          label="Currency"
          defaultValue={initialValues.unitPriceCurrency}
          {...register("unitPriceCurrency", {
            required: "Currency is required.",
          })}
          error={!!errors.unitPriceCurrency}
          helperText={errors.unitPriceCurrency?.message}
        >
          <MenuItem value="BRL">BRL - Brazilian Real</MenuItem>
          <MenuItem value="CNY">CNY - Chinese Yuan</MenuItem>
          <MenuItem value="EUR">EUR - Euro</MenuItem>
          <MenuItem value="IDR">IDR - Indonesian Rupiah</MenuItem>
          <MenuItem value="INR">INR - Indian Rupee</MenuItem>
          <MenuItem value="JPY">JPY - Japanese Yen</MenuItem>
          <MenuItem value="MXN">MXN - Mexican Peso</MenuItem>
          <MenuItem value="USD">USD - US Dollar</MenuItem>
        </TextField>

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
