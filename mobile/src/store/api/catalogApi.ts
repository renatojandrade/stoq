import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export type ProductUnit = 'Units' | 'Grams' | 'Kilograms' | 'Millilitres' | 'Litres';

export interface ProductDto {
  id: string;
  householdId: string;
  name: string;
  brand: string | null;
  category: string;
  unit: ProductUnit;
  minQuantity: number;
  imageUrl: string | null;
  barcode: string | null;
}

export interface CreateProductRequest {
  name: string;
  brand?: string | null;
  category: string;
  unit: ProductUnit;
  minQuantity: number;
  imageUrl?: string | null;
  barcode?: string | null;
}

export interface UpdateProductRequest {
  name: string;
  brand?: string | null;
  category: string;
  unit: ProductUnit;
  minQuantity: number;
  imageUrl?: string | null;
  barcode?: string | null;
}

export interface ListProductsParams {
  name?: string;
  category?: string;
}

export interface ProductLookupDto {
  barcode: string;
  name: string;
  brand: string | null;
  category: string | null;
  imageUrl: string | null;
}

export const catalogApi = createApi({
  reducerPath: 'catalogApi',
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
  tagTypes: ['Product'],
  endpoints: (builder) => ({
    listProducts: builder.query<ProductDto[], ListProductsParams | void>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        if (params?.name) searchParams.set('name', params.name);
        if (params?.category) searchParams.set('category', params.category);
        const qs = searchParams.toString();
        return `/api/products${qs ? `?${qs}` : ''}`;
      },
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Product' as const, id })), { type: 'Product', id: 'LIST' }]
          : [{ type: 'Product', id: 'LIST' }],
    }),
    getProduct: builder.query<ProductDto, string>({
      query: (id) => `/api/products/${id}`,
      providesTags: (_result, _err, id) => [{ type: 'Product', id }],
    }),
    createProduct: builder.mutation<ProductDto, CreateProductRequest>({
      query: (body) => ({ url: '/api/products', method: 'POST', body }),
      invalidatesTags: [{ type: 'Product', id: 'LIST' }],
    }),
    updateProduct: builder.mutation<void, { id: string } & UpdateProductRequest>({
      query: ({ id, ...body }) => ({ url: `/api/products/${id}`, method: 'PUT', body }),
      invalidatesTags: (_result, _err, { id }) => [
        { type: 'Product', id },
        { type: 'Product', id: 'LIST' },
      ],
    }),
    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({ url: `/api/products/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _err, id) => [
        { type: 'Product', id },
        { type: 'Product', id: 'LIST' },
      ],
    }),
    lookupByBarcode: builder.query<ProductLookupDto, string>({
      query: (code) => `/api/products/barcode/${encodeURIComponent(code)}`,
    }),
  }),
});

export const {
  useListProductsQuery,
  useGetProductQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  useLookupByBarcodeQuery,
  useLazyLookupByBarcodeQuery,
} = catalogApi;
