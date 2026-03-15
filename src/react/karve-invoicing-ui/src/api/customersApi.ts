import { apiClient } from "./apiClient";
import type {
  ApiResponse,
  PagedResult,
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  PagedQueryParams,
} from "../types/api";

const BASE = "/api/customers";

export async function getCustomers(
  params?: PagedQueryParams
): Promise<PagedResult<CustomerDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<CustomerDto>>>(
    BASE,
    { params }
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch customers.");
  }
  return data.data;
}

export async function getCustomer(id: string): Promise<CustomerDto> {
  const { data } = await apiClient.get<ApiResponse<CustomerDto>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch customer.");
  }
  return data.data;
}

export async function createCustomer(
  request: CreateCustomerRequest
): Promise<CustomerDto> {
  const { data } = await apiClient.post<ApiResponse<CustomerDto>>(
    BASE,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to create customer.");
  }
  return data.data;
}

export async function updateCustomer(
  id: string,
  request: UpdateCustomerRequest
): Promise<CustomerDto> {
  const { data } = await apiClient.put<ApiResponse<CustomerDto>>(
    `${BASE}/${id}`,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to update customer.");
  }
  return data.data;
}

export async function deleteCustomer(id: string): Promise<void> {
  const { data } = await apiClient.delete<ApiResponse<null>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess) {
    throw new Error(data.error ?? "Failed to delete customer.");
  }
}
