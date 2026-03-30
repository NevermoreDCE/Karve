import { useState } from "react";
import { useEffect } from "react";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { Alert, Box, Button, Stack, Typography } from "@mui/material";
import { DataTable, type Column } from "../components/DataTable";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { Modal } from "../components/Modal";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { ProductForm, type ProductFormValues } from "../components/ProductForm";
import { useSnackbar } from "../hooks/useSnackbar";
import {
  useCreateProduct,
  useDeleteProduct,
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
  const { enqueueSnackbar } = useSnackbar();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const productsQuery = useProducts({ page: page + 1, pageSize });
  const createProductMutation = useCreateProduct();
  const updateProductMutation = useUpdateProduct();
  const deleteProductMutation = useDeleteProduct();

  const [modalOpen, setModalOpen] = useState(false);
  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  const [form, setForm] = useState<ProductFormValues>(emptyForm);
  const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null);
  const [deleteTargetName, setDeleteTargetName] = useState<string>("");

  const isEditing = editingProductId !== null;

  const openCreate = () => {
    setEditingProductId(null);
    setForm(emptyForm);
    setModalOpen(true);
  };

  const openEdit = (product: {
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
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingProductId(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (values: ProductFormValues) => {
    if (isEditing && editingProductId) {
      await updateProductMutation.mutateAsync({ id: editingProductId, data: values });
    } else {
      await createProductMutation.mutateAsync(values);
    }
    closeModal();
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTargetId) return;
    await deleteProductMutation.mutateAsync(deleteTargetId);
    setDeleteTargetId(null);
  };

  useEffect(() => {
    if (createProductMutation.isSuccess) enqueueSnackbar("Product created.", { variant: "success" });
  }, [createProductMutation.isSuccess, enqueueSnackbar]);

  useEffect(() => {
    if (updateProductMutation.isSuccess) enqueueSnackbar("Product updated.", { variant: "success" });
  }, [updateProductMutation.isSuccess, enqueueSnackbar]);

  useEffect(() => {
    if (deleteProductMutation.isSuccess) enqueueSnackbar("Product deleted.", { variant: "success" });
  }, [deleteProductMutation.isSuccess, enqueueSnackbar]);

  const rows = productsQuery.data?.items ?? [];

  const columns: Column<(typeof rows)[number]>[] = [
    { id: "name", label: "Name" },
    { id: "sku", label: "SKU" },
    {
      id: "price",
      label: "Price",
      render: (product) => `${product.unitPriceAmount} ${product.unitPriceCurrency}`,
    },
    {
      id: "actions",
      label: "Actions",
      align: "right",
      render: (product) => (
        <Stack direction="row" spacing={1} justifyContent="flex-end">
          <Button size="small" variant="contained" startIcon={<EditOutlinedIcon />} onClick={() => openEdit(product)}>
            Edit
          </Button>
          <Button
            size="small"
            color="error"
            variant="contained"
            startIcon={<DeleteOutlineIcon />}
            onClick={() => {
              setDeleteTargetId(product.id);
              setDeleteTargetName(product.name);
            }}
          >
            Delete
          </Button>
        </Stack>
      ),
    },
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Products</Typography>
          <Typography variant="body2" color="text.secondary">Manage billable products and service catalog entries.</Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>Create Product</Button>
      </Stack>

      {productsQuery.isLoading ? <LoadingSpinner label="Loading products..." /> : null}
      {productsQuery.isError ? <Alert severity="error">{productsQuery.error.message}</Alert> : null}

      {!productsQuery.isLoading && productsQuery.data ? (
        <DataTable
          columns={columns}
          rows={rows}
          keySelector={(row) => row.id}
          page={page}
          pageSize={pageSize}
          totalCount={productsQuery.data.totalCount}
          onPageChange={setPage}
          onPageSizeChange={(nextPageSize) => {
            setPageSize(nextPageSize);
            setPage(0);
          }}
          emptyMessage="No products found."
        />
      ) : null}

      <Modal isOpen={modalOpen} onClose={closeModal} title={isEditing ? "Edit Product" : "Create Product"}>
        <ProductForm
          key={editingProductId ?? "product-create"}
          initialValues={form}
          onSubmit={handleSubmit}
          submitLabel={isEditing ? "Save Product" : "Create Product"}
          isSubmitting={createProductMutation.isPending || updateProductMutation.isPending}
          onCancel={closeModal}
        />

        {(createProductMutation.isError || updateProductMutation.isError) ? (
          <Alert severity="error" sx={{ mt: 1.5 }}>
            {createProductMutation.error?.message ?? updateProductMutation.error?.message}
          </Alert>
        ) : null}
      </Modal>

      <ConfirmDialog
        isOpen={deleteTargetId !== null}
        title="Delete Product"
        message={`Are you sure you want to delete "${deleteTargetName}"? This cannot be undone.`}
        confirmLabel="Delete"
        isDangerous
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTargetId(null)}
      />
    </Stack>
  );
}
