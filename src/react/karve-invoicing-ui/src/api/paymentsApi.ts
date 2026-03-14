import { apiClient } from "./apiClient";
import type {
  ApiResponse,
  PagedResult,
  PaymentDto,
  CreatePaymentRequest,
  UpdatePaymentRequest,
  PagedQueryParams,
} from "../types/api";

const BASE = "/api/payments";

export async function getPayments(
  params?: PagedQueryParams
): Promise<PagedResult<PaymentDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<PaymentDto>>>(
    BASE,
    { params }
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch payments.");
  }
  return data.data;
}

export async function getPayment(id: string): Promise<PaymentDto> {
  const { data } = await apiClient.get<ApiResponse<PaymentDto>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch payment.");
  }
  return data.data;
}

export async function createPayment(
  request: CreatePaymentRequest
): Promise<PaymentDto> {
  const { data } = await apiClient.post<ApiResponse<PaymentDto>>(
    BASE,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to create payment.");
  }
  return data.data;
}

export async function updatePayment(
  id: string,
  request: UpdatePaymentRequest
): Promise<PaymentDto> {
  const { data } = await apiClient.put<ApiResponse<PaymentDto>>(
    `${BASE}/${id}`,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to update payment.");
  }
  return data.data;
}

export async function deletePayment(id: string): Promise<void> {
  const { data } = await apiClient.delete<ApiResponse<null>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess) {
    throw new Error(data.error ?? "Failed to delete payment.");
  }
}
