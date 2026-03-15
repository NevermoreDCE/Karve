import {
  useQuery,
  useMutation,
  useQueryClient,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getPayments,
  getPayment,
  createPayment,
  updatePayment,
  deletePayment,
} from "../api/paymentsApi";
import type {
  PaymentDto,
  PagedResult,
  CreatePaymentRequest,
  UpdatePaymentRequest,
  PagedQueryParams,
} from "../types/api";

// ─── Query keys ──────────────────────────────────────────────────────────────

export const paymentKeys = {
  all: ["payments"] as const,
  lists: () => [...paymentKeys.all, "list"] as const,
  list: (params: PagedQueryParams) => [...paymentKeys.lists(), params] as const,
  details: () => [...paymentKeys.all, "detail"] as const,
  detail: (id: string) => [...paymentKeys.details(), id] as const,
  byInvoice: (invoiceId: string) =>
    [...paymentKeys.all, "byInvoice", invoiceId] as const,
};

// ─── Queries ─────────────────────────────────────────────────────────────────

export function usePayments(
  params?: PagedQueryParams,
  options?: Omit<
    UseQueryOptions<PagedResult<PaymentDto>>,
    "queryKey" | "queryFn"
  >
) {
  return useQuery<PagedResult<PaymentDto>>({
    queryKey: paymentKeys.list(params ?? {}),
    queryFn: () => getPayments(params),
    ...options,
  });
}

export function usePayment(
  id: string,
  options?: Omit<UseQueryOptions<PaymentDto>, "queryKey" | "queryFn">
) {
  return useQuery<PaymentDto>({
    queryKey: paymentKeys.detail(id),
    queryFn: () => getPayment(id),
    enabled: !!id,
    ...options,
  });
}

// ─── Mutations ───────────────────────────────────────────────────────────────

export function useCreatePayment() {
  const qc = useQueryClient();
  return useMutation<PaymentDto, Error, CreatePaymentRequest>({
    mutationFn: createPayment,
    onSuccess: (created) => {
      qc.invalidateQueries({ queryKey: paymentKeys.lists() });
      // Also invalidate the parent invoice so payment totals refresh.
      qc.invalidateQueries({
        queryKey: ["invoices", "detail", created.invoiceId],
      });
    },
  });
}

export function useUpdatePayment() {
  const qc = useQueryClient();
  return useMutation<
    PaymentDto,
    Error,
    { id: string; data: UpdatePaymentRequest }
  >({
    mutationFn: ({ id, data }) => updatePayment(id, data),
    onMutate: async ({ id, data }) => {
      await qc.cancelQueries({ queryKey: paymentKeys.detail(id) });
      const previous = qc.getQueryData<PaymentDto>(paymentKeys.detail(id));
      if (previous) {
        qc.setQueryData<PaymentDto>(paymentKeys.detail(id), {
          ...previous,
          ...data,
        });
      }
      return { previous };
    },
    onError: (_err, { id }, context) => {
      const ctx = context as { previous?: PaymentDto };
      if (ctx?.previous) {
        qc.setQueryData(paymentKeys.detail(id), ctx.previous);
      }
    },
    onSettled: (_data, _err, { id }) => {
      qc.invalidateQueries({ queryKey: paymentKeys.detail(id) });
      qc.invalidateQueries({ queryKey: paymentKeys.lists() });
    },
  });
}

export function useDeletePayment() {
  const qc = useQueryClient();
  return useMutation<void, Error, string>({
    mutationFn: deletePayment,
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: paymentKeys.lists() });
      const previousLists = qc.getQueriesData<PagedResult<PaymentDto>>({
        queryKey: paymentKeys.lists(),
      });
      qc.setQueriesData<PagedResult<PaymentDto>>(
        { queryKey: paymentKeys.lists() },
        (old) =>
          old
            ? { ...old, items: old.items.filter((p) => p.id !== id) }
            : old
      );
      return { previousLists };
    },
    onError: (_err, _id, context) => {
      const ctx = context as {
        previousLists: [unknown, PagedResult<PaymentDto> | undefined][];
      };
      ctx?.previousLists?.forEach(([key, value]) => {
        qc.setQueryData(key as Parameters<typeof qc.setQueryData>[0], value);
      });
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: paymentKeys.lists() });
    },
  });
}
