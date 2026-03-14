import { useState } from "react";
import {
  useCreateCustomer,
  useCustomers,
  useUpdateCustomer,
} from "../hooks/useCustomers";
import type { CreateCustomerRequest } from "../types/api";

type CustomerFormState = CreateCustomerRequest;

const emptyForm: CustomerFormState = {
  name: "",
  email: "",
  billingAddress: "",
};

export function CustomersPage() {
  const customersQuery = useCustomers({ page: 1, pageSize: 50 });
  const createCustomerMutation = useCreateCustomer();
  const updateCustomerMutation = useUpdateCustomer();

  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null);
  const [form, setForm] = useState<CustomerFormState>(emptyForm);

  const isEditing = editingCustomerId !== null;

  const beginEdit = (customer: {
    id: string;
    name: string;
    email: string;
    billingAddress: string;
  }) => {
    setEditingCustomerId(customer.id);
    setForm({
      name: customer.name,
      email: customer.email,
      billingAddress: customer.billingAddress,
    });
  };

  const resetForm = () => {
    setEditingCustomerId(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (isEditing && editingCustomerId) {
      await updateCustomerMutation.mutateAsync({ id: editingCustomerId, data: form });
      resetForm();
      return;
    }

    await createCustomerMutation.mutateAsync(form);
    resetForm();
  };

  return (
    <section>
      <h1>Customers</h1>
      <p>Manage customer records used for invoice billing.</p>

      <form onSubmit={handleSubmit} style={{ marginBottom: 20 }}>
        <h2>{isEditing ? "Edit Customer" : "Create Customer"}</h2>

        <label>
          Name
          <input
            required
            value={form.name}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, name: event.target.value }))
            }
          />
        </label>

        <label>
          Email
          <input
            required
            type="email"
            value={form.email}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, email: event.target.value }))
            }
          />
        </label>

        <label>
          Billing Address
          <input
            required
            value={form.billingAddress}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, billingAddress: event.target.value }))
            }
          />
        </label>

        <div style={{ marginTop: 8 }}>
          <button
            type="submit"
            disabled={createCustomerMutation.isPending || updateCustomerMutation.isPending}
          >
            {isEditing
              ? updateCustomerMutation.isPending
                ? "Saving..."
                : "Save Customer"
              : createCustomerMutation.isPending
              ? "Creating..."
              : "Create Customer"}
          </button>
          {isEditing ? (
            <button type="button" style={{ marginLeft: 8 }} onClick={resetForm}>
              Cancel
            </button>
          ) : null}
        </div>
      </form>

      {customersQuery.isLoading ? <p>Loading customers...</p> : null}
      {customersQuery.isError ? <p role="alert">{customersQuery.error.message}</p> : null}

      {!customersQuery.isLoading && customersQuery.data ? (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Billing Address</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {customersQuery.data.items.length === 0 ? (
              <tr>
                <td colSpan={4}>No customers found.</td>
              </tr>
            ) : (
              customersQuery.data.items.map((customer) => (
                <tr key={customer.id}>
                  <td>{customer.name}</td>
                  <td>{customer.email}</td>
                  <td>{customer.billingAddress}</td>
                  <td>
                    <button type="button" onClick={() => beginEdit(customer)}>
                      Edit
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      ) : null}

      {createCustomerMutation.isError ? (
        <p role="alert">{createCustomerMutation.error.message}</p>
      ) : null}
      {updateCustomerMutation.isError ? (
        <p role="alert">{updateCustomerMutation.error.message}</p>
      ) : null}
    </section>
  );
}
