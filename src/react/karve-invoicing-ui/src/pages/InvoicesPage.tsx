import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import AddIcon from "@mui/icons-material/Add";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import {
  Alert,
  Box,
  Button,
  Stack,
  Typography,
} from "@mui/material";
import { InvoiceForm, type InvoiceFormValues } from "../components/InvoiceForm";
import { DataTable, type Column } from "../components/DataTable";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { Modal } from "../components/Modal";
import { StatusBadge } from "../components/StatusBadge";
import { useSnackbar } from "../hooks/useSnackbar";
import { useCreateInvoice, useInvoices } from "../hooks/useInvoices";
import { runUiSpan } from "../observability/otel";
import type { InvoiceStatus } from "../types/api";

// Map for numeric status values (if backend sends numbers)
const statusNumberToString: Record<number, InvoiceStatus> = {
  0: "Draft",
  1: "Sent",
  2: "Viewed",
  3: "Paid",
  4: "Overdue",
  5: "Canceled",
};

function toDateInputValue(isoDate: string): string {
  return isoDate.slice(0, 10);
}

function getDefaultDueDate(): string {
  const today = new Date();
  const due = new Date(today);
  due.setDate(today.getDate() + 30);
  return due.toISOString().slice(0, 10);
}

export function InvoicesPage() {
  const [showCreate, setShowCreate] = useState(false);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [orderBy, setOrderBy] = useState<"invoiceNumber" | "invoiceDate" | "dueDate">("invoiceDate");
  const [order, setOrder] = useState<"asc" | "desc">("desc");
  const { enqueueSnackbar } = useSnackbar();

  const defaultFormValues: InvoiceFormValues = {
    customerId: "",
    invoiceDate: new Date().toISOString().slice(0, 10),
    dueDate: getDefaultDueDate(),
    status: "Draft",
  };

  const invoicesQuery = useInvoices({ page: page + 1, pageSize });
  const createInvoiceMutation = useCreateInvoice();

  const handleCreate = async (values: InvoiceFormValues) => {
    await runUiSpan(
      "ui.invoice.create.submit",
      { "ui.operation": "create_invoice", "invoice.status": values.status },
      () => createInvoiceMutation.mutateAsync(values)
    );
    setShowCreate(false);
  };

  const tableRows = useMemo<
    {
      id: string;
      invoiceNumber: number;
      status: InvoiceStatus;
      companyName: string;
      customerName: string;
      invoiceDate: string;
      dueDate: string;
    }[]
  >(() => {
    if (!invoicesQuery.data) return [];
    const preparedRows = runUiSpan(
      "render.invoice.table",
      { "ui.operation": "invoice_table_prepare", "invoice.row_count": invoicesQuery.data.items.length },
      () =>
        invoicesQuery.data.items.map((invoice) => {
          // If status is a number, map to string; otherwise, use as is
          let status: InvoiceStatus;
          if (typeof invoice.status === "number") {
            status = statusNumberToString[invoice.status] ?? "Draft";
          } else {
            status = invoice.status;
          }
          return {
            id: invoice.id,
            invoiceNumber: invoice.invoiceNumber,
            status,
            companyName: invoice.companyName || invoice.companyId.slice(0, 8),
            customerName: invoice.customerName || invoice.customerId.slice(0, 8),
            invoiceDate: toDateInputValue(invoice.invoiceDate),
            dueDate: toDateInputValue(invoice.dueDate),
          };
        })
    );

    const sorted = [...preparedRows].sort((a, b) => {
      const left = a[orderBy];
      const right = b[orderBy];
      if (left === right) return 0;
      const comparison = left > right ? 1 : -1;
      return order === "asc" ? comparison : -comparison;
    });

    return sorted;
  }, [invoicesQuery.data]);

  useEffect(() => {
    if (createInvoiceMutation.isSuccess) {
      enqueueSnackbar("Invoice created.", { variant: "success" });
    }
  }, [createInvoiceMutation.isSuccess, enqueueSnackbar]);

  const columns: Column<(typeof tableRows)[number]>[] = [
    {
      id: "invoiceNumber",
      label: "#",
      sortable: true,
      render: (row) => <Typography sx={{ fontWeight: 700 }}>#{row.invoiceNumber}</Typography>,
    },
    {
      id: "status",
      label: "Status",
      render: (row) => <StatusBadge status={row.status} />,
    },
    { id: "companyName", label: "Company" },
    { id: "customerName", label: "Customer" },
    { id: "invoiceDate", label: "Invoice Date", sortable: true },
    { id: "dueDate", label: "Due Date", sortable: true },
    {
      id: "actions",
      label: "Actions",
      align: "right",
      render: (row) => (
        <Button
          component={Link}
          to={`/invoices/${row.id}`}
          variant="contained"
          size="small"
          startIcon={<VisibilityOutlinedIcon fontSize="small" />}
        >
          View
        </Button>
      ),
    },
  ];

  const handleSort = (columnId: string) => {
    if (columnId !== "invoiceNumber" && columnId !== "invoiceDate" && columnId !== "dueDate") {
      return;
    }
    if (orderBy === columnId) {
      setOrder((prev) => (prev === "asc" ? "desc" : "asc"));
      return;
    }
    setOrderBy(columnId);
    setOrder("asc");
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Invoices</Typography>
          <Typography variant="body2" color="text.secondary">
            Track invoice status, due dates, and payment progress.
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setShowCreate(true)}>
          Create Invoice
        </Button>
      </Stack>

      {invoicesQuery.isLoading ? <LoadingSpinner label="Loading invoices..." /> : null}
      {invoicesQuery.isError ? <Alert severity="error">{invoicesQuery.error.message}</Alert> : null}

      {!invoicesQuery.isLoading && invoicesQuery.data ? (
        <DataTable
          columns={columns}
          rows={tableRows}
          keySelector={(row) => row.id}
          order={order}
          orderBy={orderBy}
          onSort={handleSort}
          page={page}
          pageSize={pageSize}
          totalCount={invoicesQuery.data.totalCount}
          onPageChange={setPage}
          onPageSizeChange={(nextPageSize) => {
            setPageSize(nextPageSize);
            setPage(0);
          }}
          emptyMessage="No invoices found."
        />
      ) : null}

      <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Invoice">
        <InvoiceForm
          key={showCreate ? "create-open" : "create-closed"}
          initialValues={defaultFormValues}
          onSubmit={handleCreate}
          submitLabel="Save Invoice"
          isSubmitting={createInvoiceMutation.isPending}
          onCancel={() => setShowCreate(false)}
        />

        {createInvoiceMutation.isError ? (
          <Alert severity="error" sx={{ mt: 1.5 }}>
            {createInvoiceMutation.error.message}
          </Alert>
        ) : null}
      </Modal>
    </Stack>
  );
}

