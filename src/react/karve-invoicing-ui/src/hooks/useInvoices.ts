import {
  useQuery,
  useMutation,
  useQueryClient,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getInvoices,
  getInvoice,
  createInvoice,
  updateInvoice,
  deleteInvoice,
} from "../api/invoicesApi";
import type {
  InvoiceDto,
  PagedResult,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  PagedQueryParams,
} from "../types/api";

// ─── Query keys ──────────────────────────────────────────────────────────────

export const invoiceKeys = {
  all: ["invoices"] as const,
  lists: () => [...invoiceKeys.all, "list"] as const,
  list: (params: PagedQueryParams) => [...invoiceKeys.lists(), params] as const,
  details: () => [...invoiceKeys.all, "detail"] as const,
  detail: (id: string) => [...invoiceKeys.details(), id] as const,
};

// ─── Queries ─────────────────────────────────────────────────────────────────

export function useInvoices(
  params?: PagedQueryParams,
  options?: Omit<UseQueryOptions<PagedResult<InvoiceDto>>, "queryKey" | "queryFn">
) {
  return useQuery<PagedResult<InvoiceDto>>({
    queryKey: invoiceKeys.list(params ?? {}),
    queryFn: () => getInvoices(params),
    ...options,
  });
}

export function useInvoice(
  id: string,
  options?: Omit<UseQueryOptions<InvoiceDto>, "queryKey" | "queryFn">
) {
  return useQuery<InvoiceDto>({
    queryKey: invoiceKeys.detail(id),
    queryFn: () => getInvoice(id),
    enabled: !!id,
    ...options,
  });
}

// ─── Mutations ───────────────────────────────────────────────────────────────

export function useCreateInvoice() {
  const qc = useQueryClient();
  return useMutation<InvoiceDto, Error, CreateInvoiceRequest>({
    mutationFn: createInvoice,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: invoiceKeys.lists() });
    },
  });
}

export function useUpdateInvoice() {
  const qc = useQueryClient();
  return useMutation<
    InvoiceDto,
    Error,
    { id: string; data: UpdateInvoiceRequest }
  >({
    mutationFn: ({ id, data }) => updateInvoice(id, data),
    // E4 — Optimistic update: write into the detail cache immediately.
    onMutate: async ({ id, data }) => {
      await qc.cancelQueries({ queryKey: invoiceKeys.detail(id) });
      const previous = qc.getQueryData<InvoiceDto>(invoiceKeys.detail(id));
      if (previous) {
        qc.setQueryData<InvoiceDto>(invoiceKeys.detail(id), {
          ...previous,
          ...data,
        });
      }
      return { previous };
    },
    onError: (_err, { id }, context) => {
      const ctx = context as { previous?: InvoiceDto };
      if (ctx?.previous) {
        qc.setQueryData(invoiceKeys.detail(id), ctx.previous);
      }
    },
    onSettled: (_data, _err, { id }) => {
      qc.invalidateQueries({ queryKey: invoiceKeys.detail(id) });
      qc.invalidateQueries({ queryKey: invoiceKeys.lists() });
    },
  });
}

export function useDeleteInvoice() {
  const qc = useQueryClient();
  return useMutation<void, Error, string>({
    mutationFn: deleteInvoice,
    // E4 — Optimistic update: remove from list cache immediately.
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: invoiceKeys.lists() });
      const previousLists = qc.getQueriesData<PagedResult<InvoiceDto>>({
        queryKey: invoiceKeys.lists(),
      });
      qc.setQueriesData<PagedResult<InvoiceDto>>(
        { queryKey: invoiceKeys.lists() },
        (old) =>
          old
            ? { ...old, items: old.items.filter((i) => i.id !== id) }
            : old
      );
      return { previousLists };
    },
    onError: (_err, _id, context) => {
      const ctx = context as {
        previousLists: [unknown, PagedResult<InvoiceDto> | undefined][];
      };
      ctx?.previousLists?.forEach(([key, value]) => {
        qc.setQueryData(key as Parameters<typeof qc.setQueryData>[0], value);
      });
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: invoiceKeys.lists() });
    },
  });
}
