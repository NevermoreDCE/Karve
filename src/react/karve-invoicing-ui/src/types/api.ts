// ─── Enums (mirror backend Domain.Enums) ──────────────────────────────────────

export type InvoiceStatus =
  | "Draft"
  | "Sent"
  | "Viewed"
  | "Paid"
  | "Overdue"
  | "Canceled";

export type PaymentMethod =
  | "CreditCard"
  | "ACH"
  | "Check"
  | "Cash"
  | "Other";

// ─── Shared response wrappers (mirror Application.Responses) ─────────────────

export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  error?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── Domain DTOs (mirror Application.DTOs) ───────────────────────────────────

export interface InvoiceLineItemDto {
  id: string;
  invoiceId: string;
  productId: string;
  quantity: number;
  unitPriceAmount: number;
  unitPriceCurrency: string;
}

export interface PaymentDto {
  id: string;
  companyId: string;
  invoiceId: string;
  amount: number;
  currency: string;
  paymentDate: string; // ISO-8601
  method: PaymentMethod;
}

export interface InvoiceDto {
  id: string;
  companyId: string;
  customerId: string;
  invoiceDate: string; // ISO-8601
  dueDate: string;     // ISO-8601
  status: InvoiceStatus;
  lineItems: InvoiceLineItemDto[];
  payments: PaymentDto[];
}

export interface CustomerDto {
  id: string;
  companyId: string;
  name: string;
  email: string;
  billingAddress: string;
}

export interface ProductDto {
  id: string;
  companyId: string;
  name: string;
  sku: string;
  unitPriceAmount: number;
  unitPriceCurrency: string;
}

// ─── Request models (mirror Application.DTOs request types) ──────────────────

export interface CreateInvoiceRequest {
  customerId: string;
  invoiceDate: string;
  dueDate: string;
  status?: InvoiceStatus;
}

export interface UpdateInvoiceRequest {
  customerId: string;
  invoiceDate: string;
  dueDate: string;
  status: InvoiceStatus;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  billingAddress: string;
}

export interface UpdateCustomerRequest {
  name: string;
  email: string;
  billingAddress: string;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  unitPriceAmount: number;
  unitPriceCurrency: string;
}

export interface UpdateProductRequest {
  name: string;
  sku: string;
  unitPriceAmount: number;
  unitPriceCurrency: string;
}

export interface CreatePaymentRequest {
  invoiceId: string;
  amount: number;
  currency: string;
  paymentDate: string;
  method: PaymentMethod;
}

export interface UpdatePaymentRequest {
  invoiceId: string;
  amount: number;
  currency: string;
  paymentDate: string;
  method: PaymentMethod;
}

export interface CreateInvoiceLineItemRequest {
  invoiceId: string;
  productId: string;
  quantity: number;
  unitPriceAmount: number;
  unitPriceCurrency: string;
}

// ─── Query params ─────────────────────────────────────────────────────────────

export interface PagedQueryParams {
  page?: number;
  pageSize?: number;
  filter?: string;
}
