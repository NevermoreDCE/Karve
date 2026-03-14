import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useDeleteInvoice,
  useInvoice,
  useUpdateInvoice,
} from "../hooks/useInvoices";
import type { InvoiceStatus, UpdateInvoiceRequest } from "../types/api";

const invoiceStatuses: InvoiceStatus[] = [
  "Draft",
  "Sent",
  "Viewed",
  "Paid",
  "Overdue",
  "Canceled",
];

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
  const [form, setForm] = useState<UpdateInvoiceRequest>({
    customerId: "",
    invoiceDate: "",
    dueDate: "",
    status: "Draft",
  });

  useEffect(() => {
    if (!invoiceQuery.data) {
      return;
    }

    setForm({
      customerId: invoiceQuery.data.customerId,
      invoiceDate: toDateInputValue(invoiceQuery.data.invoiceDate),
      dueDate: toDateInputValue(invoiceQuery.data.dueDate),
      status: invoiceQuery.data.status,
    });
  }, [invoiceQuery.data]);

  const handleUpdate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!invoiceId) {
      return;
    }
    await updateInvoiceMutation.mutateAsync({ id: invoiceId, data: form });
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
    return <p>Loading invoice details...</p>;
  }

  if (invoiceQuery.isError) {
    return <p role="alert">{invoiceQuery.error.message}</p>;
  }

  if (!invoiceQuery.data) {
    return <p>Invoice not found.</p>;
  }

  const invoice = invoiceQuery.data;

  return (
    <section>
      <p>
        <Link to="/invoices">Back to Invoices</Link>
      </p>

      <h1>Invoice Details</h1>

      {isEditing ? (
        <form onSubmit={handleUpdate} style={{ marginBottom: 20 }}>
          <h2>Edit Invoice</h2>

          <label>
            Customer ID
            <input
              required
              value={form.customerId}
              onChange={(event) =>
                setForm((prev) => ({ ...prev, customerId: event.target.value }))
              }
            />
          </label>

          <label>
            Invoice Date
            <input
              required
              type="date"
              value={form.invoiceDate}
              onChange={(event) =>
                setForm((prev) => ({ ...prev, invoiceDate: event.target.value }))
              }
            />
          </label>

          <label>
            Due Date
            <input
              required
              type="date"
              value={form.dueDate}
              onChange={(event) =>
                setForm((prev) => ({ ...prev, dueDate: event.target.value }))
              }
            />
          </label>

          <label>
            Status
            <select
              value={form.status}
              onChange={(event) =>
                setForm((prev) => ({
                  ...prev,
                  status: event.target.value as InvoiceStatus,
                }))
              }
            >
              {invoiceStatuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>

          <div style={{ marginTop: 8 }}>
            <button type="submit" disabled={updateInvoiceMutation.isPending}>
              {updateInvoiceMutation.isPending ? "Saving..." : "Save Changes"}
            </button>
            <button
              type="button"
              style={{ marginLeft: 8 }}
              onClick={() => setIsEditing(false)}
            >
              Cancel
            </button>
          </div>
        </form>
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