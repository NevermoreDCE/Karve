import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import toast from "react-hot-toast";
import { InvoiceForm, type InvoiceFormValues } from "../components/InvoiceForm";
import { LoadingSpinner } from "../components/LoadingSpinner";
import {
  useDeleteInvoice,
  useInvoice,
  useUpdateInvoice,
} from "../hooks/useInvoices";

function toDateInputValue(isoDate: string): string {
  return isoDate.slice(0, 10);
}

export function InvoiceDetailPage() {
  const params = useParams<{ id: string }>();
  const navigate = useNavigate();
  const invoiceId = params.id ?? "";

  const invoiceQuery = useInvoice(invoiceId);
  const updateInvoiceMutation = useUpdateInvoice();
  const deleteInvoiceMutation = useDeleteInvoice();

  const [isEditing, setIsEditing] = useState(false);

  const handleUpdate = async (values: InvoiceFormValues) => {
    if (!invoiceId) {
      return;
    }
    await updateInvoiceMutation.mutateAsync({ id: invoiceId, data: values });
    setIsEditing(false);
  };

  const handleDelete = async () => {
    if (!invoiceId) {
      return;
    }

    const approved = window.confirm("Delete this invoice?");
    if (!approved) {
      return;
    }

    await deleteInvoiceMutation.mutateAsync(invoiceId);
    toast.success("Invoice deleted.");
    navigate("/invoices");
  };

  if (!invoiceId) {
    return (
      <section>
        <h1>Invoice Details</h1>
        <p>Missing invoice id.</p>
      </section>
    );
  }

  if (invoiceQuery.isLoading) {
    return <LoadingSpinner label="Loading invoice details..." />;
  }

  if (invoiceQuery.isError) {
    return <p role="alert">{invoiceQuery.error.message}</p>;
  }

  if (!invoiceQuery.data) {
    return <p>Invoice not found.</p>;
  }

  const invoice = invoiceQuery.data;
  const invoiceFormInitialValues: InvoiceFormValues = {
    customerId: invoice.customerId,
    invoiceDate: toDateInputValue(invoice.invoiceDate),
    dueDate: toDateInputValue(invoice.dueDate),
    status: invoice.status,
  };

  return (
    <section>
      <p>
        <Link to="/invoices">Back to Invoices</Link>
      </p>

      <h1>Invoice Details</h1>

      {isEditing ? (
        <div style={{ marginBottom: 20 }}>
          <h2>Edit Invoice</h2>
          <InvoiceForm
            key={`invoice-edit-${invoice.id}`}
            initialValues={invoiceFormInitialValues}
            onSubmit={handleUpdate}
            submitLabel="Save Changes"
            isSubmitting={updateInvoiceMutation.isPending}
            onCancel={() => setIsEditing(false)}
          />
        </div>
      ) : (
        <div style={{ marginBottom: 20 }}>
          <p>
            <strong>Invoice ID:</strong> {invoice.id}
          </p>
          <p>
            <strong>Company:</strong> {invoice.companyId}
          </p>
          <p>
            <strong>Customer:</strong> {invoice.customerId}
          </p>
          <p>
            <strong>Status:</strong> {invoice.status}
          </p>
          <p>
            <strong>Invoice Date:</strong> {toDateInputValue(invoice.invoiceDate)}
          </p>
          <p>
            <strong>Due Date:</strong> {toDateInputValue(invoice.dueDate)}
          </p>

          <button type="button" onClick={() => setIsEditing(true)}>
            Edit Invoice
          </button>
          <button
            type="button"
            style={{ marginLeft: 8 }}
            onClick={handleDelete}
            disabled={deleteInvoiceMutation.isPending}
          >
            {deleteInvoiceMutation.isPending ? "Deleting..." : "Delete Invoice"}
          </button>
        </div>
      )}

      <h2>Line Items</h2>
      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Qty</th>
            <th>Unit Price</th>
          </tr>
        </thead>
        <tbody>
          {invoice.lineItems.length === 0 ? (
            <tr>
              <td colSpan={3}>No line items.</td>
            </tr>
          ) : (
            invoice.lineItems.map((lineItem) => (
              <tr key={lineItem.id}>
                <td>{lineItem.productId.slice(0, 8)}...</td>
                <td>{lineItem.quantity}</td>
                <td>
                  {lineItem.unitPriceAmount} {lineItem.unitPriceCurrency}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      <h2 style={{ marginTop: 20 }}>Payments</h2>
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Method</th>
            <th>Amount</th>
          </tr>
        </thead>
        <tbody>
          {invoice.payments.length === 0 ? (
            <tr>
              <td colSpan={3}>No payments recorded.</td>
            </tr>
          ) : (
            invoice.payments.map((payment) => (
              <tr key={payment.id}>
                <td>{toDateInputValue(payment.paymentDate)}</td>
                <td>{payment.method}</td>
                <td>
                  {payment.amount} {payment.currency}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      {updateInvoiceMutation.isError ? (
        <p role="alert">{updateInvoiceMutation.error.message}</p>
      ) : null}
      {deleteInvoiceMutation.isError ? (
        <p role="alert">{deleteInvoiceMutation.error.message}</p>
      ) : null}
    </section>
  );
}