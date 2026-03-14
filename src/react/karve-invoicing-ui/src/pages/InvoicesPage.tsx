import { useState } from "react";
import { Link } from "react-router-dom";
import { InvoiceForm, type InvoiceFormValues } from "../components/InvoiceForm";
import { useCreateInvoice, useInvoices } from "../hooks/useInvoices";

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
  const defaultFormValues: InvoiceFormValues = {
    customerId: "",
    invoiceDate: new Date().toISOString().slice(0, 10),
    dueDate: getDefaultDueDate(),
    status: "Draft",
  };

  const invoicesQuery = useInvoices({ page: 1, pageSize: 20 });
  const createInvoiceMutation = useCreateInvoice();

  const handleCreate = async (values: InvoiceFormValues) => {
    await createInvoiceMutation.mutateAsync(values);
    setShowCreate(false);
  };

  return (
    <section>
      <h1>Invoices</h1>
      <p>Track invoice status, due dates, and payment progress.</p>

      <button type="button" onClick={() => setShowCreate((value) => !value)}>
        {showCreate ? "Cancel" : "Create Invoice"}
      </button>

      {showCreate ? (
        <div style={{ marginTop: 12, marginBottom: 24 }}>
          <h2>New Invoice</h2>
          <InvoiceForm
            key={showCreate ? "create-open" : "create-closed"}
            initialValues={defaultFormValues}
            onSubmit={handleCreate}
            submitLabel="Save Invoice"
            isSubmitting={createInvoiceMutation.isPending}
            onCancel={() => setShowCreate(false)}
          />

          {createInvoiceMutation.isError ? (
            <p role="alert">{createInvoiceMutation.error.message}</p>
          ) : null}
        </div>
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
