import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export interface CategoryDto {
  id: string;
  nameEn: string;
  namePt: string;
}

export interface BrandDto {
  id: string;
  name: string;
}

export interface UnitDto {
  id: string;
  code: string;
  nameEn: string;
  namePt: string;
}

export interface ProductDto {
  id: string;
  householdId: string | null;
  name: string;
  categoryId: string;
  categoryNameEn: string;
  categoryNamePt: string;
  brandId: string | null;
  brandName: string | null;
  unitId: string;
  unitCode: string;
  unitNameEn: string;
  unitNamePt: string;
  minQuantity: number;
  imageUrl: string | null;
  barcode: string | null;
  country: string | null;
  isReference: boolean;
}

export interface CreateProductRequest {
  name: string;
  categoryId: string;
  brandName?: string | null;
  unitId: string;
  minQuantity: number;
  imageUrl?: string | null;
  barcode?: string | null;
}

export interface UpdateProductRequest {
  name: string;
  categoryId: string;
  brandName?: string | null;
  unitId: string;
  minQuantity: number;
  imageUrl?: string | null;
  barcode?: string | null;
}

export interface ListProductsParams {
  name?: string;
  categoryId?: string;
  isReference?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ProductLookupDto {
  barcode: string;
  name: string;
  brand: string | null;
  category: string | null;
  imageUrl: string | null;
  existingProductId: string | null;
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
  tagTypes: ['Product', 'Category', 'Unit', 'Brand'],
  endpoints: (builder) => ({
    listCategories: builder.query<CategoryDto[], void>({
      query: () => '/api/products/categories',
      providesTags: [{ type: 'Category', id: 'LIST' }],
    }),
    listUnits: builder.query<UnitDto[], void>({
      query: () => '/api/products/units',
      providesTags: [{ type: 'Unit', id: 'LIST' }],
    }),
    listBrands: builder.query<BrandDto[], string | void>({
      query: (name) => `/api/products/brands${name ? `?name=${encodeURIComponent(name)}` : ''}`,
      providesTags: [{ type: 'Brand', id: 'LIST' }],
    }),
    listProducts: builder.query<ProductDto[], ListProductsParams | void>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        if (params?.name) searchParams.set('name', params.name);
        if (params?.categoryId) searchParams.set('categoryId', params.categoryId);
        if (params?.isReference !== undefined) searchParams.set('isReference', String(params.isReference));
        if (params?.page) searchParams.set('page', String(params.page));
        if (params?.pageSize) searchParams.set('pageSize', String(params.pageSize));
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
  useListCategoriesQuery,
  useListUnitsQuery,
  useListBrandsQuery,
  useListProductsQuery,
  useGetProductQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  useLookupByBarcodeQuery,
  useLazyLookupByBarcodeQuery,
} = catalogApi;
