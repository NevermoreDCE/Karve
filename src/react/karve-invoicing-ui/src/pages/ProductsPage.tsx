import { useState } from "react";
import { useEffect } from "react";
import toast from "react-hot-toast";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { ProductForm, type ProductFormValues } from "../components/ProductForm";
import {
  useCreateProduct,
  useProducts,
  useUpdateProduct,
} from "../hooks/useProducts";

const emptyForm: ProductFormValues = {
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
  const [form, setForm] = useState<ProductFormValues>(emptyForm);

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

  const handleSubmit = async (values: ProductFormValues) => {
    if (isEditing && editingProductId) {
      await updateProductMutation.mutateAsync({ id: editingProductId, data: values });
      resetForm();
      return;
    }

    await createProductMutation.mutateAsync(values);
    resetForm();
  };

  useEffect(() => {
    if (createProductMutation.isSuccess) {
      toast.success("Product created.");
    }
  }, [createProductMutation.isSuccess]);

  useEffect(() => {
    if (updateProductMutation.isSuccess) {
      toast.success("Product updated.");
    }
  }, [updateProductMutation.isSuccess]);

  return (
    <section>
      <h1>Products</h1>
      <p>Manage billable products and service catalog entries.</p>

      <div style={{ marginBottom: 20 }}>
        <h2>{isEditing ? "Edit Product" : "Create Product"}</h2>
        <ProductForm
          key={editingProductId ?? "product-create"}
          initialValues={form}
          onSubmit={handleSubmit}
          submitLabel={isEditing ? "Save Product" : "Create Product"}
          isSubmitting={createProductMutation.isPending || updateProductMutation.isPending}
          onCancel={isEditing ? resetForm : undefined}
        />
      </div>

      {productsQuery.isLoading ? <LoadingSpinner label="Loading products..." /> : null}
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
