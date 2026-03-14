import { useState } from "react";
import {
  useCreateProduct,
  useProducts,
  useUpdateProduct,
} from "../hooks/useProducts";
import type { CreateProductRequest } from "../types/api";

type ProductFormState = CreateProductRequest;

const emptyForm: ProductFormState = {
  name: "",
  sku: "",
  unitPriceAmount: 0,
  unitPriceCurrency: "USD",
};

export function ProductsPage() {
  const productsQuery = useProducts({ page: 1, pageSize: 50 });
  const createProductMutation = useCreateProduct();
  const updateProductMutation = useUpdateProduct();

  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  const [form, setForm] = useState<ProductFormState>(emptyForm);

  const isEditing = editingProductId !== null;

  const beginEdit = (product: {
    id: string;
    name: string;
    sku: string;
    unitPriceAmount: number;
    unitPriceCurrency: string;
  }) => {
    setEditingProductId(product.id);
    setForm({
      name: product.name,
      sku: product.sku,
      unitPriceAmount: product.unitPriceAmount,
      unitPriceCurrency: product.unitPriceCurrency,
    });
  };

  const resetForm = () => {
    setEditingProductId(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (isEditing && editingProductId) {
      await updateProductMutation.mutateAsync({ id: editingProductId, data: form });
      resetForm();
      return;
    }

    await createProductMutation.mutateAsync(form);
    resetForm();
  };

  return (
    <section>
      <h1>Products</h1>
      <p>Manage billable products and service catalog entries.</p>

      <form onSubmit={handleSubmit} style={{ marginBottom: 20 }}>
        <h2>{isEditing ? "Edit Product" : "Create Product"}</h2>

        <label>
          Name
          <input
            required
            value={form.name}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, name: event.target.value }))
            }
          />
        </label>

        <label>
          SKU
          <input
            required
            value={form.sku}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, sku: event.target.value }))
            }
          />
        </label>

        <label>
          Unit Price
          <input
            required
            type="number"
            min="0"
            step="0.01"
            value={form.unitPriceAmount}
            onChange={(event) =>
              setForm((prev) => ({
                ...prev,
                unitPriceAmount: Number(event.target.value),
              }))
            }
          />
        </label>

        <label>
          Currency
          <input
            required
            maxLength={3}
            value={form.unitPriceCurrency}
            onChange={(event) =>
              setForm((prev) => ({
                ...prev,
                unitPriceCurrency: event.target.value.toUpperCase(),
              }))
            }
          />
        </label>

        <div style={{ marginTop: 8 }}>
          <button
            type="submit"
            disabled={createProductMutation.isPending || updateProductMutation.isPending}
          >
            {isEditing
              ? updateProductMutation.isPending
                ? "Saving..."
                : "Save Product"
              : createProductMutation.isPending
              ? "Creating..."
              : "Create Product"}
          </button>
          {isEditing ? (
            <button type="button" style={{ marginLeft: 8 }} onClick={resetForm}>
              Cancel
            </button>
          ) : null}
        </div>
      </form>

      {productsQuery.isLoading ? <p>Loading products...</p> : null}
      {productsQuery.isError ? <p role="alert">{productsQuery.error.message}</p> : null}

      {!productsQuery.isLoading && productsQuery.data ? (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>SKU</th>
              <th>Price</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {productsQuery.data.items.length === 0 ? (
              <tr>
                <td colSpan={4}>No products found.</td>
              </tr>
            ) : (
              productsQuery.data.items.map((product) => (
                <tr key={product.id}>
                  <td>{product.name}</td>
                  <td>{product.sku}</td>
                  <td>
                    {product.unitPriceAmount} {product.unitPriceCurrency}
                  </td>
                  <td>
                    <button type="button" onClick={() => beginEdit(product)}>
                      Edit
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      ) : null}

      {createProductMutation.isError ? (
        <p role="alert">{createProductMutation.error.message}</p>
      ) : null}
      {updateProductMutation.isError ? (
        <p role="alert">{updateProductMutation.error.message}</p>
      ) : null}
    </section>
  );
}
