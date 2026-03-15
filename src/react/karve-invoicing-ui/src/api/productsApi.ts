import { apiClient } from "./apiClient";
import type {
  ApiResponse,
  PagedResult,
  ProductDto,
  CreateProductRequest,
  UpdateProductRequest,
  PagedQueryParams,
} from "../types/api";

const BASE = "/api/products";

export async function getProducts(
  params?: PagedQueryParams
): Promise<PagedResult<ProductDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<ProductDto>>>(
    BASE,
    { params }
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch products.");
  }
  return data.data;
}

export async function getProduct(id: string): Promise<ProductDto> {
  const { data } = await apiClient.get<ApiResponse<ProductDto>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch product.");
  }
  return data.data;
}

export async function createProduct(
  request: CreateProductRequest
): Promise<ProductDto> {
  const { data } = await apiClient.post<ApiResponse<ProductDto>>(
    BASE,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to create product.");
  }
  return data.data;
}

export async function updateProduct(
  id: string,
  request: UpdateProductRequest
): Promise<ProductDto> {
  const { data } = await apiClient.put<ApiResponse<ProductDto>>(
    `${BASE}/${id}`,
    request
  );
  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to update product.");
  }
  return data.data;
}

export async function deleteProduct(id: string): Promise<void> {
  const { data } = await apiClient.delete<ApiResponse<null>>(
    `${BASE}/${id}`
  );
  if (!data.isSuccess) {
    throw new Error(data.error ?? "Failed to delete product.");
  }
}
