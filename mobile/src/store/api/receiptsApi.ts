import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export type ParseCorrectionStatus =
  | 'Pending'
  | 'Processing'
  | 'Parsed'
  | 'Confirmed'
  | 'Failed';

export interface ParsedReceiptItemDto {
  productName: string;
  quantity: number;
  unit: string;
  barcode: string | null;
  retailerSku?: string | null;
  unitPrice?: number | null;
  totalPrice?: number | null;
  discount?: number | null;
  correctionItemId?: string;
  matchedProductId?: string | null;
}

export interface ParseReceiptResultDto {
  correctionId: string;
  retailerName: string | null;
  items: ParsedReceiptItemDto[];
}

export interface ParseCorrectionSummaryDto {
  id: string;
  status: ParseCorrectionStatus;
  retailerName: string | null;
  receiptNumber: string | null;
  emittedAt: string | null;
  parsedItemCount: number;
  createdAt: string;
  parsedAt: string | null;
  failureReason: string | null;
}

export interface ParseCorrectionDetailDto {
  id: string;
  status: ParseCorrectionStatus;
  retailerName: string | null;
  receiptNumber: string | null;
  emittedAt: string | null;
  createdAt: string;
  items: ParsedReceiptItemDto[];
}

export interface ParseReceiptPhotoRequest {
  imageBase64: string;
  contentType?: string;
}

export interface ParseNfeReceiptRequest {
  qrCodeUrl: string;
}

export interface NfeJobAcceptedDto {
  correctionId: string;
}

export interface BatchInventoryItemRequest {
  productName: string;
  quantity: number;
  unit: string;
  expiryDate?: string | null;
  notes?: string | null;
  correctionItemId?: string | null;
  matchedProductId?: string | null;
  barcode?: string | null;
  retailerSku?: string | null;
}

export interface BatchAddInventoryItemsRequest {
  items: BatchInventoryItemRequest[];
  correctionId?: string | null;
}

export const receiptsApi = createApi({
  reducerPath: 'receiptsApi',
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
  tagTypes: ['ParseCorrection'],
  endpoints: (builder) => ({
    parseReceiptPhoto: builder.mutation<ParseReceiptResultDto, ParseReceiptPhotoRequest>({
      query: (body) => ({ url: '/api/receipts/parse-photo', method: 'POST', body }),
    }),
    parseNfeReceipt: builder.mutation<NfeJobAcceptedDto, ParseNfeReceiptRequest>({
      query: (body) => ({ url: '/api/receipts/parse-nfe', method: 'POST', body }),
      invalidatesTags: [{ type: 'ParseCorrection', id: 'LIST' }],
    }),
    getCorrections: builder.query<ParseCorrectionSummaryDto[], number | void>({
      query: (limit = 20) => `/api/receipts/corrections?limit=${limit}`,
      providesTags: (result) =>
        result
          ? [
              ...result.map(({ id }) => ({ type: 'ParseCorrection' as const, id })),
              { type: 'ParseCorrection', id: 'LIST' },
            ]
          : [{ type: 'ParseCorrection', id: 'LIST' }],
    }),
    getCorrectionDetail: builder.query<ParseCorrectionDetailDto, string>({
      query: (correctionId) => `/api/receipts/corrections/${correctionId}`,
      providesTags: (_result, _err, correctionId) => [
        { type: 'ParseCorrection', id: correctionId },
      ],
    }),
    batchAddInventoryItems: builder.mutation<void, BatchAddInventoryItemsRequest>({
      query: (body) => ({ url: '/api/inventory/batch', method: 'POST', body }),
      invalidatesTags: (_result, _err, arg) =>
        arg.correctionId
          ? [{ type: 'ParseCorrection', id: arg.correctionId }]
          : [],
    }),
  }),
});

export const {
  useParseReceiptPhotoMutation,
  useParseNfeReceiptMutation,
  useGetCorrectionsQuery,
  useGetCorrectionDetailQuery,
  useBatchAddInventoryItemsMutation,
} = receiptsApi;
