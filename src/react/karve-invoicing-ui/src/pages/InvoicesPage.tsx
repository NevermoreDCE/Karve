import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { InvoiceForm, type InvoiceFormValues } from "../components/InvoiceForm";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useCreateInvoice, useInvoices } from "../hooks/useInvoices";
import { runUiSpan } from "../observability/otel";

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
    await runUiSpan(
      "ui.invoice.create.submit",
      {
        "ui.operation": "create_invoice",
        "invoice.status": values.status,
      },
      () => createInvoiceMutation.mutateAsync(values)
    );

    setShowCreate(false);
  };

  const tableRows = useMemo(() => {
    if (!invoicesQuery.data) {
      return [];
    }

    return runUiSpan(
      "render.invoice.table",
      {
        "ui.operation": "invoice_table_prepare",
        "invoice.row_count": invoicesQuery.data.items.length,
      },
      () =>
        invoicesQuery.data.items.map((invoice) => ({
          id: invoice.id,
          idPreview: `${invoice.id.slice(0, 8)}...`,
          status: invoice.status,
          customerPreview: `${invoice.customerId.slice(0, 8)}...`,
          invoiceDate: toDateInputValue(invoice.invoiceDate),
          dueDate: toDateInputValue(invoice.dueDate),
        }))
    );
  }, [invoicesQuery.data]);

  useEffect(() => {
    if (createInvoiceMutation.isSuccess) {
      toast.success("Invoice created.");
    }
  }, [createInvoiceMutation.isSuccess]);

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

      {invoicesQuery.isLoading ? <LoadingSpinner label="Loading invoices..." /> : null}
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
            {tableRows.length === 0 ? (
              <tr>
                <td colSpan={6}>No invoices found.</td>
              </tr>
            ) : (
              tableRows.map((row) => (
                <tr key={row.id}>
                  <td>{row.idPreview}</td>
                  <td>{row.status}</td>
                  <td>{row.customerPreview}</td>
                  <td>{row.invoiceDate}</td>
                  <td>{row.dueDate}</td>
                  <td>
                    <Link to={`/invoices/${row.id}`}>View Details</Link>
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
