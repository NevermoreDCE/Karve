import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { InvoiceForm, type InvoiceFormValues } from "../components/InvoiceForm";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { StatusBadge } from "../components/StatusBadge";
import { useSnackbar } from "../hooks/useSnackbar";
import {
  useDeleteInvoice,
  useInvoice,
  useUpdateInvoice,
} from "../hooks/useInvoices";

function toDateInputValue(isoDate: string): string {
  return isoDate.slice(0, 10);
}

function DetailRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <Stack direction={{ xs: "column", sm: "row" }} spacing={0.75} sx={{ py: 0.75, borderBottom: 1, borderColor: "divider" }}>
      <Typography sx={{ width: { sm: 130 }, fontWeight: 700 }} color="text.secondary">{label}</Typography>
      <Typography variant="body2">{children}</Typography>
    </Stack>
  );
}

export function InvoiceDetailPage() {
  const params = useParams<{ id: string }>();
  const navigate = useNavigate();
  const invoiceId = params.id ?? "";

  const invoiceQuery = useInvoice(invoiceId);
  const updateInvoiceMutation = useUpdateInvoice();
  const deleteInvoiceMutation = useDeleteInvoice();
  const { enqueueSnackbar } = useSnackbar();

  const [isEditing, setIsEditing] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);

  const handleUpdate = async (values: InvoiceFormValues) => {
    if (!invoiceId) return;
    await updateInvoiceMutation.mutateAsync({ id: invoiceId, data: values });
    setIsEditing(false);
  };

  const handleDeleteConfirm = async () => {
    if (!invoiceId) return;
    setConfirmDelete(false);
    await deleteInvoiceMutation.mutateAsync(invoiceId);
    enqueueSnackbar("Invoice deleted.", { variant: "success" });
    navigate("/invoices");
  };

  if (!invoiceId) {
    return (
      <Stack spacing={1}>
        <Typography variant="h4">Invoice Details</Typography>
        <Typography color="text.secondary">Missing invoice id.</Typography>
      </Stack>
    );
  }

  if (invoiceQuery.isLoading) return <LoadingSpinner label="Loading invoice details..." />;
  if (invoiceQuery.isError) return <Alert severity="error">{invoiceQuery.error.message}</Alert>;
  if (!invoiceQuery.data) return <Alert severity="info">Invoice not found.</Alert>;

  const invoice = invoiceQuery.data;
  const invoiceFormInitialValues: InvoiceFormValues = {
    customerId: invoice.customerId,
    invoiceDate: toDateInputValue(invoice.invoiceDate),
    dueDate: toDateInputValue(invoice.dueDate),
    status: invoice.status,
  };

  return (
    <Stack spacing={2}>
      <Button component={Link} to="/invoices" variant="text" startIcon={<ArrowBackIcon />} sx={{ alignSelf: "flex-start" }}>
        Back to Invoices
      </Button>

      <Typography variant="h4" component="h1">Invoice #{invoice.invoiceNumber}</Typography>

      {isEditing ? (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="h6" sx={{ mb: 1.5 }}>Edit Invoice</Typography>
          <InvoiceForm
            key={`invoice-edit-${invoice.id}`}
            initialValues={invoiceFormInitialValues}
            onSubmit={handleUpdate}
            submitLabel="Save Changes"
            isSubmitting={updateInvoiceMutation.isPending}
            onCancel={() => setIsEditing(false)}
          />
        </Paper>
      ) : (
        <Stack spacing={2}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <DetailRow label="Invoice #">#{invoice.invoiceNumber}</DetailRow>
            <DetailRow label="Company">{invoice.companyName || invoice.companyId}</DetailRow>
            <DetailRow label="Customer">{invoice.customerName || invoice.customerId}</DetailRow>
            <DetailRow label="Status"><StatusBadge status={invoice.status} /></DetailRow>
            <DetailRow label="Invoice Date">{toDateInputValue(invoice.invoiceDate)}</DetailRow>
            <DetailRow label="Due Date">{toDateInputValue(invoice.dueDate)}</DetailRow>
          </Paper>

          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<EditOutlinedIcon />} onClick={() => setIsEditing(true)}>
              Edit Invoice
            </Button>
            <Button
              color="error"
              variant="contained"
              startIcon={<DeleteOutlineIcon />}
              onClick={() => setConfirmDelete(true)}
              disabled={deleteInvoiceMutation.isPending}
            >
              {deleteInvoiceMutation.isPending ? "Deleting..." : "Delete Invoice"}
            </Button>
          </Stack>
        </Stack>
      )}

      <Dialog open={confirmDelete} onClose={() => setConfirmDelete(false)}>
        <DialogTitle>Delete Invoice?</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary">
              Are you sure you want to delete Invoice #{invoice.invoiceNumber}? This cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button variant="outlined" onClick={() => setConfirmDelete(false)}>Cancel</Button>
          <Button color="error" variant="contained" onClick={handleDeleteConfirm}>Delete</Button>
        </DialogActions>
      </Dialog>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="h6" sx={{ mb: 1 }}>Line Items</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Product</TableCell>
              <TableCell align="right">Qty</TableCell>
              <TableCell align="right">Unit Price</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
          {invoice.lineItems.length === 0 ? (
            <TableRow>
              <TableCell colSpan={3} align="center">
                No line items.
              </TableCell>
            </TableRow>
          ) : (
            invoice.lineItems.map((lineItem) => (
              <TableRow key={lineItem.id}>
                <TableCell>{lineItem.productId.slice(0, 8)}...</TableCell>
                <TableCell align="right">{lineItem.quantity}</TableCell>
                <TableCell align="right">{lineItem.unitPriceAmount} {lineItem.unitPriceCurrency}</TableCell>
              </TableRow>
            ))
          )}
          </TableBody>
        </Table>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="h6" sx={{ mb: 1 }}>Payments</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Method</TableCell>
              <TableCell align="right">Amount</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
          {invoice.payments.length === 0 ? (
            <TableRow>
              <TableCell colSpan={3} align="center">
                No payments recorded.
              </TableCell>
            </TableRow>
          ) : (
            invoice.payments.map((payment) => (
              <TableRow key={payment.id}>
                <TableCell>{toDateInputValue(payment.paymentDate)}</TableCell>
                <TableCell>{payment.method}</TableCell>
                <TableCell align="right">{payment.amount} {payment.currency}</TableCell>
              </TableRow>
            ))
          )}
          </TableBody>
        </Table>
      </Paper>

      {updateInvoiceMutation.isError ? (
        <Alert severity="error">{updateInvoiceMutation.error.message}</Alert>
      ) : null}
      {deleteInvoiceMutation.isError ? (
        <Alert severity="error">{deleteInvoiceMutation.error.message}</Alert>
      ) : null}
    </Stack>
  );
}

