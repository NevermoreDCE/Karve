import { useState } from "react";
import { Link } from "react-router-dom";
import { useCreateInvoice, useInvoices } from "../hooks/useInvoices";
import type { CreateInvoiceRequest, InvoiceStatus } from "../types/api";

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

function getDefaultDueDate(): string {
  const today = new Date();
  const due = new Date(today);
  due.setDate(today.getDate() + 30);
  return due.toISOString().slice(0, 10);
}

export function InvoicesPage() {
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState<CreateInvoiceRequest>({
    customerId: "",
    invoiceDate: new Date().toISOString().slice(0, 10),
    dueDate: getDefaultDueDate(),
    status: "Draft",
  });

  const invoicesQuery = useInvoices({ page: 1, pageSize: 20 });
  const createInvoiceMutation = useCreateInvoice();

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await createInvoiceMutation.mutateAsync(form);
    setShowCreate(false);
    setForm({
      customerId: "",
      invoiceDate: new Date().toISOString().slice(0, 10),
      dueDate: getDefaultDueDate(),
      status: "Draft",
    });
  };

  return (
    <section>
      <h1>Invoices</h1>
      <p>Track invoice status, due dates, and payment progress.</p>

      <button type="button" onClick={() => setShowCreate((value) => !value)}>
        {showCreate ? "Cancel" : "Create Invoice"}
      </button>

      {showCreate ? (
        <form onSubmit={handleCreate} style={{ marginTop: 12, marginBottom: 24 }}>
          <h2>New Invoice</h2>

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
            <button type="submit" disabled={createInvoiceMutation.isPending}>
              {createInvoiceMutation.isPending ? "Creating..." : "Save Invoice"}
            </button>
          </div>

          {createInvoiceMutation.isError ? (
            <p role="alert">{createInvoiceMutation.error.message}</p>
          ) : null}
        </form>
      ) : null}

      {invoicesQuery.isLoading ? <p>Loading invoices...</p> : null}
      {invoicesQuery.isError ? <p role="alert">{invoicesQuery.error.message}</p> : null}

      {!invoicesQuery.isLoading && invoicesQuery.data ? (
        <table>
          <thead>
            <tr>
              <th>Invoice</th>
              <th>Status</th>
              <th>Customer</th>
              <th>Invoice Date</th>
              <th>Due Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {invoicesQuery.data.items.length === 0 ? (
              <tr>
                <td colSpan={6}>No invoices found.</td>
              </tr>
            ) : (
              invoicesQuery.data.items.map((invoice) => (
                <tr key={invoice.id}>
                  <td>{invoice.id.slice(0, 8)}...</td>
                  <td>{invoice.status}</td>
                  <td>{invoice.customerId.slice(0, 8)}...</td>
                  <td>{toDateInputValue(invoice.invoiceDate)}</td>
                  <td>{toDateInputValue(invoice.dueDate)}</td>
                  <td>
                    <Link to={`/invoices/${invoice.id}`}>View Details</Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      ) : null}
    </section>
  );
}
