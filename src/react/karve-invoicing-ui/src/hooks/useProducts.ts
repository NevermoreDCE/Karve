import {
  useQuery,
  useMutation,
  useQueryClient,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getProducts,
  getProduct,
  createProduct,
  updateProduct,
  deleteProduct,
} from "../api/productsApi";
import type {
  ProductDto,
  PagedResult,
  CreateProductRequest,
  UpdateProductRequest,
  PagedQueryParams,
} from "../types/api";

// ─── Query keys ──────────────────────────────────────────────────────────────

export const productKeys = {
  all: ["products"] as const,
  lists: () => [...productKeys.all, "list"] as const,
  list: (params: PagedQueryParams) => [...productKeys.lists(), params] as const,
  details: () => [...productKeys.all, "detail"] as const,
  detail: (id: string) => [...productKeys.details(), id] as const,
};

// ─── Queries ─────────────────────────────────────────────────────────────────

export function useProducts(
  params?: PagedQueryParams,
  options?: Omit<
    UseQueryOptions<PagedResult<ProductDto>>,
    "queryKey" | "queryFn"
  >
) {
  return useQuery<PagedResult<ProductDto>>({
    queryKey: productKeys.list(params ?? {}),
    queryFn: () => getProducts(params),
    ...options,
  });
}

export function useProduct(
  id: string,
  options?: Omit<UseQueryOptions<ProductDto>, "queryKey" | "queryFn">
) {
  return useQuery<ProductDto>({
    queryKey: productKeys.detail(id),
    queryFn: () => getProduct(id),
    enabled: !!id,
    ...options,
  });
}

// ─── Mutations ───────────────────────────────────────────────────────────────

export function useCreateProduct() {
  const qc = useQueryClient();
  return useMutation<ProductDto, Error, CreateProductRequest>({
    mutationFn: createProduct,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}

export function useUpdateProduct() {
  const qc = useQueryClient();
  return useMutation<
    ProductDto,
    Error,
    { id: string; data: UpdateProductRequest }
  >({
    mutationFn: ({ id, data }) => updateProduct(id, data),
    onMutate: async ({ id, data }) => {
      await qc.cancelQueries({ queryKey: productKeys.detail(id) });
      const previous = qc.getQueryData<ProductDto>(productKeys.detail(id));
      if (previous) {
        qc.setQueryData<ProductDto>(productKeys.detail(id), {
          ...previous,
          ...data,
        });
      }
      return { previous };
    },
    onError: (_err, { id }, context) => {
      const ctx = context as { previous?: ProductDto };
      if (ctx?.previous) {
        qc.setQueryData(productKeys.detail(id), ctx.previous);
      }
    },
    onSettled: (_data, _err, { id }) => {
      qc.invalidateQueries({ queryKey: productKeys.detail(id) });
      qc.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}

export function useDeleteProduct() {
  const qc = useQueryClient();
  return useMutation<void, Error, string>({
    mutationFn: deleteProduct,
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: productKeys.lists() });
      const previousLists = qc.getQueriesData<PagedResult<ProductDto>>({
        queryKey: productKeys.lists(),
      });
      qc.setQueriesData<PagedResult<ProductDto>>(
        { queryKey: productKeys.lists() },
        (old) =>
          old
            ? { ...old, items: old.items.filter((p) => p.id !== id) }
            : old
      );
      return { previousLists };
    },
    onError: (_err, _id, context) => {
      const ctx = context as {
        previousLists: [unknown, PagedResult<ProductDto> | undefined][];
      };
      ctx?.previousLists?.forEach(([key, value]) => {
        qc.setQueryData(key as Parameters<typeof qc.setQueryData>[0], value);
      });
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}
