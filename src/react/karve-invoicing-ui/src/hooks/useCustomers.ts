import {
  useQuery,
  useMutation,
  useQueryClient,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getCustomers,
  getCustomer,
  createCustomer,
  updateCustomer,
  deleteCustomer,
} from "../api/customersApi";
import type {
  CustomerDto,
  PagedResult,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  PagedQueryParams,
} from "../types/api";

// ─── Query keys ──────────────────────────────────────────────────────────────

export const customerKeys = {
  all: ["customers"] as const,
  lists: () => [...customerKeys.all, "list"] as const,
  list: (params: PagedQueryParams) =>
    [...customerKeys.lists(), params] as const,
  details: () => [...customerKeys.all, "detail"] as const,
  detail: (id: string) => [...customerKeys.details(), id] as const,
};

// ─── Queries ─────────────────────────────────────────────────────────────────

export function useCustomers(
  params?: PagedQueryParams,
  options?: Omit<
    UseQueryOptions<PagedResult<CustomerDto>>,
    "queryKey" | "queryFn"
  >
) {
  return useQuery<PagedResult<CustomerDto>>({
    queryKey: customerKeys.list(params ?? {}),
    queryFn: () => getCustomers(params),
    ...options,
  });
}

export function useCustomer(
  id: string,
  options?: Omit<UseQueryOptions<CustomerDto>, "queryKey" | "queryFn">
) {
  return useQuery<CustomerDto>({
    queryKey: customerKeys.detail(id),
    queryFn: () => getCustomer(id),
    enabled: !!id,
    ...options,
  });
}

// ─── Mutations ───────────────────────────────────────────────────────────────

export function useCreateCustomer() {
  const qc = useQueryClient();
  return useMutation<CustomerDto, Error, CreateCustomerRequest>({
    mutationFn: createCustomer,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: customerKeys.lists() });
    },
  });
}

export function useUpdateCustomer() {
  const qc = useQueryClient();
  return useMutation<
    CustomerDto,
    Error,
    { id: string; data: UpdateCustomerRequest }
  >({
    mutationFn: ({ id, data }) => updateCustomer(id, data),
    onMutate: async ({ id, data }) => {
      await qc.cancelQueries({ queryKey: customerKeys.detail(id) });
      const previous = qc.getQueryData<CustomerDto>(customerKeys.detail(id));
      if (previous) {
        qc.setQueryData<CustomerDto>(customerKeys.detail(id), {
          ...previous,
          ...data,
        });
      }
      return { previous };
    },
    onError: (_err, { id }, context) => {
      const ctx = context as { previous?: CustomerDto };
      if (ctx?.previous) {
        qc.setQueryData(customerKeys.detail(id), ctx.previous);
      }
    },
    onSettled: (_data, _err, { id }) => {
      qc.invalidateQueries({ queryKey: customerKeys.detail(id) });
      qc.invalidateQueries({ queryKey: customerKeys.lists() });
    },
  });
}

export function useDeleteCustomer() {
  const qc = useQueryClient();
  return useMutation<void, Error, string>({
    mutationFn: deleteCustomer,
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: customerKeys.lists() });
      const previousLists = qc.getQueriesData<PagedResult<CustomerDto>>({
        queryKey: customerKeys.lists(),
      });
      qc.setQueriesData<PagedResult<CustomerDto>>(
        { queryKey: customerKeys.lists() },
        (old) =>
          old
            ? { ...old, items: old.items.filter((c) => c.id !== id) }
            : old
      );
      return { previousLists };
    },
    onError: (_err, _id, context) => {
      const ctx = context as {
        previousLists: [unknown, PagedResult<CustomerDto> | undefined][];
      };
      ctx?.previousLists?.forEach(([key, value]) => {
        qc.setQueryData(key as Parameters<typeof qc.setQueryData>[0], value);
      });
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: customerKeys.lists() });
    },
  });
}
