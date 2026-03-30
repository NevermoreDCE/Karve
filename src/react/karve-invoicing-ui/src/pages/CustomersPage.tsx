import { useState } from "react";
import { useEffect } from "react";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { Alert, Box, Button, Stack, Typography } from "@mui/material";
import {
  CustomerForm,
  type CustomerFormValues,
} from "../components/CustomerForm";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { DataTable, type Column } from "../components/DataTable";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { Modal } from "../components/Modal";
import { useSnackbar } from "../hooks/useSnackbar";
import {
  useCreateCustomer,
  useCustomers,
  useDeleteCustomer,
  useUpdateCustomer,
} from "../hooks/useCustomers";

const emptyForm: CustomerFormValues = {
  name: "",
  email: "",
  billingAddress: "",
};

export function CustomersPage() {
  const { enqueueSnackbar } = useSnackbar();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const customersQuery = useCustomers({ page: page + 1, pageSize });
  const createCustomerMutation = useCreateCustomer();
  const updateCustomerMutation = useUpdateCustomer();
  const deleteCustomerMutation = useDeleteCustomer();

  const [modalOpen, setModalOpen] = useState(false);
  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null);
  const [form, setForm] = useState<CustomerFormValues>(emptyForm);
  const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null);
  const [deleteTargetName, setDeleteTargetName] = useState<string>("");

  const isEditing = editingCustomerId !== null;

  const openCreate = () => {
    setEditingCustomerId(null);
    setForm(emptyForm);
    setModalOpen(true);
  };

  const openEdit = (customer: {
    id: string;
    name: string;
    email: string;
    billingAddress: string;
  }) => {
    setEditingCustomerId(customer.id);
    setForm({ name: customer.name, email: customer.email, billingAddress: customer.billingAddress });
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingCustomerId(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (values: CustomerFormValues) => {
    if (isEditing && editingCustomerId) {
      await updateCustomerMutation.mutateAsync({ id: editingCustomerId, data: values });
    } else {
      await createCustomerMutation.mutateAsync(values);
    }
    closeModal();
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTargetId) return;
    await deleteCustomerMutation.mutateAsync(deleteTargetId);
    setDeleteTargetId(null);
  };

  useEffect(() => {
    if (createCustomerMutation.isSuccess) enqueueSnackbar("Customer created.", { variant: "success" });
  }, [createCustomerMutation.isSuccess, enqueueSnackbar]);

  useEffect(() => {
    if (updateCustomerMutation.isSuccess) enqueueSnackbar("Customer updated.", { variant: "success" });
  }, [updateCustomerMutation.isSuccess, enqueueSnackbar]);

  useEffect(() => {
    if (deleteCustomerMutation.isSuccess) enqueueSnackbar("Customer deleted.", { variant: "success" });
  }, [deleteCustomerMutation.isSuccess, enqueueSnackbar]);

  const rows = customersQuery.data?.items ?? [];

  const columns: Column<(typeof rows)[number]>[] = [
    { id: "name", label: "Name" },
    { id: "email", label: "Email" },
    { id: "billingAddress", label: "Billing Address" },
    {
      id: "actions",
      label: "Actions",
      align: "right",
      render: (customer) => (
        <Stack direction="row" spacing={1} justifyContent="flex-end">
          <Button size="small" variant="contained" startIcon={<EditOutlinedIcon />} onClick={() => openEdit(customer)}>
            Edit
          </Button>
          <Button
            size="small"
            color="error"
            variant="contained"
            startIcon={<DeleteOutlineIcon />}
            onClick={() => {
              setDeleteTargetId(customer.id);
              setDeleteTargetName(customer.name);
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
          <Typography variant="h4" component="h1">Customers</Typography>
          <Typography variant="body2" color="text.secondary">Manage customer records used for invoice billing.</Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>Create Customer</Button>
      </Stack>

      {customersQuery.isLoading ? <LoadingSpinner label="Loading customers..." /> : null}
      {customersQuery.isError ? <Alert severity="error">{customersQuery.error.message}</Alert> : null}

      {!customersQuery.isLoading && customersQuery.data ? (
        <DataTable
          columns={columns}
          rows={rows}
          keySelector={(row) => row.id}
          page={page}
          pageSize={pageSize}
          totalCount={customersQuery.data.totalCount}
          onPageChange={setPage}
          onPageSizeChange={(nextPageSize) => {
            setPageSize(nextPageSize);
            setPage(0);
          }}
          emptyMessage="No customers found."
        />
      ) : null}

      <Modal isOpen={modalOpen} onClose={closeModal} title={isEditing ? "Edit Customer" : "Create Customer"}>
        <CustomerForm
          key={editingCustomerId ?? "customer-create"}
          initialValues={form}
          onSubmit={handleSubmit}
          submitLabel={isEditing ? "Save Customer" : "Create Customer"}
          isSubmitting={createCustomerMutation.isPending || updateCustomerMutation.isPending}
          onCancel={closeModal}
        />

        {(createCustomerMutation.isError || updateCustomerMutation.isError) ? (
          <Alert severity="error" sx={{ mt: 1.5 }}>
            {createCustomerMutation.error?.message ?? updateCustomerMutation.error?.message}
          </Alert>
        ) : null}
      </Modal>

      <ConfirmDialog
        isOpen={deleteTargetId !== null}
        title="Delete Customer"
        message={`Are you sure you want to delete "${deleteTargetName}"? This cannot be undone.`}
        confirmLabel="Delete"
        isDangerous
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTargetId(null)}
      />
    </Stack>
  );
}

