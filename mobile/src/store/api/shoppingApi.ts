import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export interface ShoppingListItemDto {
  id: string;
  householdId: string;
  productName: string;
  quantity: number;
  unit: string;
  notes: string | null;
  isChecked: boolean;
  source: 'Manual' | 'Auto';
  createdAt: string;
}

export interface AddShoppingListItemRequest {
  productName: string;
  quantity: number;
  unit: string;
  notes?: string | null;
}

export const shoppingApi = createApi({
  reducerPath: 'shoppingApi',
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
  tagTypes: ['ShoppingItem'],
  endpoints: (builder) => ({
    listShopping: builder.query<ShoppingListItemDto[], void>({
      query: () => '/api/shopping',
      providesTags: (result) =>
        result
          ? [
              ...result.map(({ id }) => ({ type: 'ShoppingItem' as const, id })),
              { type: 'ShoppingItem', id: 'LIST' },
            ]
          : [{ type: 'ShoppingItem', id: 'LIST' }],
    }),
    addShoppingItem: builder.mutation<ShoppingListItemDto, AddShoppingListItemRequest>({
      query: (body) => ({ url: '/api/shopping', method: 'POST', body }),
      invalidatesTags: [{ type: 'ShoppingItem', id: 'LIST' }],
    }),
    checkOffItem: builder.mutation<ShoppingListItemDto, string>({
      query: (id) => ({ url: `/api/shopping/${id}/check`, method: 'POST' }),
      invalidatesTags: (_result, _err, id) => [
        { type: 'ShoppingItem', id },
        { type: 'ShoppingItem', id: 'LIST' },
      ],
    }),
    uncheckItem: builder.mutation<ShoppingListItemDto, string>({
      query: (id) => ({ url: `/api/shopping/${id}/uncheck`, method: 'POST' }),
      invalidatesTags: (_result, _err, id) => [
        { type: 'ShoppingItem', id },
        { type: 'ShoppingItem', id: 'LIST' },
      ],
    }),
    removeShoppingItem: builder.mutation<void, string>({
      query: (id) => ({ url: `/api/shopping/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _err, id) => [
        { type: 'ShoppingItem', id },
        { type: 'ShoppingItem', id: 'LIST' },
      ],
    }),
    clearCheckedItems: builder.mutation<void, void>({
      query: () => ({ url: '/api/shopping/checked', method: 'DELETE' }),
      invalidatesTags: [{ type: 'ShoppingItem', id: 'LIST' }],
    }),
  }),
});

export const {
  useListShoppingQuery,
  useAddShoppingItemMutation,
  useCheckOffItemMutation,
  useUncheckItemMutation,
  useRemoveShoppingItemMutation,
  useClearCheckedItemsMutation,
} = shoppingApi;
