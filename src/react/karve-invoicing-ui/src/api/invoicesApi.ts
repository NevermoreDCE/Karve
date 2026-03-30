import { apiClient } from "./apiClient";
import type {
  ApiResponse,
  PagedResult,
  InvoiceDto,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  PagedQueryParams,
} from "../types/api";

const BASE = "/api/invoices";

export async function getInvoices(
  params?: PagedQueryParams
): Promise<PagedResult<InvoiceDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<InvoiceDto>>>(
    BASE,
    { params }
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch invoices.");
  }
  return data.data;
}

export async function getInvoice(id: string): Promise<InvoiceDto> {
  const { data } = await apiClient.get<ApiResponse<InvoiceDto>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch invoice.");
  }
  return data.data;
}

export async function createInvoice(
  request: CreateInvoiceRequest
): Promise<InvoiceDto> {
  const { data } = await apiClient.post<ApiResponse<InvoiceDto>>(
    BASE,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to create invoice.");
  }
  return data.data;
}

export async function updateInvoice(
  id: string,
  request: UpdateInvoiceRequest
): Promise<InvoiceDto> {
  const { data } = await apiClient.put<ApiResponse<InvoiceDto>>(
    `${BASE}/${id}`,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to update invoice.");
  }
  return data.data;
}

export async function deleteInvoice(id: string): Promise<void> {
  const { data } = await apiClient.delete<ApiResponse<null>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess) {
    throw new Error(data.error ?? "Failed to delete invoice.");
  }
}

export interface SendInvoiceEmailResponse {
  jobId: string;
  invoiceId: string;
  message: string;
}

export async function sendInvoiceEmail(
  invoiceId: string
): Promise<SendInvoiceEmailResponse> {
  const { data } = await apiClient.post<ApiResponse<SendInvoiceEmailResponse>>(
    `${BASE}/${invoiceId}/send-email`
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to send invoice email.");
  }
  return data.data;
}

export interface RunOverdueCheckResponse {
  jobsEnqueued: number;
  companyIds: string[];
  message: string;
}

export async function runOverdueCheck(
  companyId?: string
): Promise<RunOverdueCheckResponse> {
  const { data } = await apiClient.post<ApiResponse<RunOverdueCheckResponse>>(
    `${BASE}/overdue-check/run`,
    {},
    { params: companyId ? { companyId } : {} }
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to trigger overdue check.");
  }
  return data.data;
}
