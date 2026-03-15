import { apiClient } from "./apiClient";
import type { ApiResponse, CompanyDto, PagedResult } from "../types/api";

export async function getCompanies(): Promise<CompanyDto[]> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<CompanyDto>>>(
    "/api/companies",
    {
      params: {
        page: 1,
        pageSize: 100,
      },
    }
  );

  if (!data.isSuccess || !data.data) {
    throw new Error(data.error ?? "Failed to fetch companies.");
  }

  return data.data.items;
}
