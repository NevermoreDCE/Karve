import { useState } from "react";
import { useEffect } from "react";
import toast from "react-hot-toast";
import {
  CustomerForm,
  type CustomerFormValues,
} from "../components/CustomerForm";
import { LoadingSpinner } from "../components/LoadingSpinner";
import {
  useCreateCustomer,
  useCustomers,
  useUpdateCustomer,
} from "../hooks/useCustomers";

const emptyForm: CustomerFormValues = {
  name: "",
  email: "",
  billingAddress: "",
};

export function CustomersPage() {
  const customersQuery = useCustomers({ page: 1, pageSize: 50 });
  const createCustomerMutation = useCreateCustomer();
  const updateCustomerMutation = useUpdateCustomer();

  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null);
  const [form, setForm] = useState<CustomerFormValues>(emptyForm);

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

  const handleSubmit = async (values: CustomerFormValues) => {
    if (isEditing && editingCustomerId) {
      await updateCustomerMutation.mutateAsync({ id: editingCustomerId, data: values });
      resetForm();
      return;
    }

    await createCustomerMutation.mutateAsync(values);
    resetForm();
  };

  useEffect(() => {
    if (createCustomerMutation.isSuccess) {
      toast.success("Customer created.");
    }
  }, [createCustomerMutation.isSuccess]);

  useEffect(() => {
    if (updateCustomerMutation.isSuccess) {
      toast.success("Customer updated.");
    }
  }, [updateCustomerMutation.isSuccess]);

  return (
    <section>
      <h1>Customers</h1>
      <p>Manage customer records used for invoice billing.</p>

      <div style={{ marginBottom: 20 }}>
        <h2>{isEditing ? "Edit Customer" : "Create Customer"}</h2>
        <CustomerForm
          key={editingCustomerId ?? "customer-create"}
          initialValues={form}
          onSubmit={handleSubmit}
          submitLabel={isEditing ? "Save Customer" : "Create Customer"}
          isSubmitting={createCustomerMutation.isPending || updateCustomerMutation.isPending}
          onCancel={isEditing ? resetForm : undefined}
        />
      </div>

      {customersQuery.isLoading ? <LoadingSpinner label="Loading customers..." /> : null}
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
