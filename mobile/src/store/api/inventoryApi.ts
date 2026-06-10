import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export interface InventoryItemDto {
  id: string;
  householdId: string;
  productId: string | null;
  productName: string;
  productBrand: string | null;
  productImageUrl: string | null;
  quantity: number;
  minQuantity: number;
  unit: string;
  expiryDate: string | null;
  notes: string | null;
  isStockLow: boolean;
}

export interface AddInventoryItemRequest {
  productId: string;
  quantity: number;
  unit: string;
  expiryDate?: string | null;
  notes?: string | null;
}

export interface ConsumeInventoryItemRequest {
  amount: number;
}

export interface ListInventoryParams {
  name?: string;
  productId?: string;
}

export const inventoryApi = createApi({
  reducerPath: 'inventoryApi',
  baseQuery: fetchBaseQuery({
    baseUrl: BASE_URL,
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.accessToken;
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['InventoryItem'],
  endpoints: (builder) => ({
    listInventory: builder.query<InventoryItemDto[], ListInventoryParams | void>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        if (params?.name) searchParams.set('name', params.name);
        if (params?.productId) searchParams.set('productId', params.productId);
        const qs = searchParams.toString();
        return `/api/inventory${qs ? `?${qs}` : ''}`;
      },
      providesTags: (result) =>
        result
          ? [
              ...result.map(({ id }) => ({ type: 'InventoryItem' as const, id })),
              { type: 'InventoryItem', id: 'LIST' },
            ]
          : [{ type: 'InventoryItem', id: 'LIST' }],
    }),
    getExpiringItems: builder.query<InventoryItemDto[], number | void>({
      query: (daysAhead = 7) => `/api/inventory/expiring?daysAhead=${daysAhead}`,
      providesTags: [{ type: 'InventoryItem', id: 'EXPIRING' }],
    }),
    addInventoryItem: builder.mutation<InventoryItemDto, AddInventoryItemRequest>({
      query: (body) => ({ url: '/api/inventory', method: 'POST', body }),
      invalidatesTags: [
        { type: 'InventoryItem', id: 'LIST' },
        { type: 'InventoryItem', id: 'EXPIRING' },
      ],
    }),
    consumeInventoryItem: builder.mutation<
      InventoryItemDto,
      { id: string } & ConsumeInventoryItemRequest
    >({
      query: ({ id, ...body }) => ({
        url: `/api/inventory/${id}/consume`,
        method: 'POST',
        body,
      }),
      async onQueryStarted({ id, amount }, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          inventoryApi.util.updateQueryData('listInventory', undefined, (draft) => {
            const item = draft.find((i) => i.id === id);
            if (item) { item.quantity = Math.max(0, item.quantity - amount); }
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _err, { id }) => [
        { type: 'InventoryItem', id },
        { type: 'InventoryItem', id: 'LIST' },
        { type: 'InventoryItem', id: 'EXPIRING' },
      ],
    }),
    removeInventoryItem: builder.mutation<void, string>({
      query: (id) => ({ url: `/api/inventory/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _err, id) => [
        { type: 'InventoryItem', id },
        { type: 'InventoryItem', id: 'LIST' },
        { type: 'InventoryItem', id: 'EXPIRING' },
      ],
    }),
    triggerExpiryScan: builder.mutation<number, void>({
      query: () => ({ url: '/api/inventory/expiry-scan', method: 'POST' }),
      invalidatesTags: [{ type: 'InventoryItem', id: 'EXPIRING' }],
    }),
  }),
});

export const {
  useListInventoryQuery,
  useGetExpiringItemsQuery,
  useAddInventoryItemMutation,
  useConsumeInventoryItemMutation,
  useRemoveInventoryItemMutation,
  useTriggerExpiryScanMutation,
} = inventoryApi;
