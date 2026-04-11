import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../index';

// Base URL — override via EXPO_PUBLIC_API_URL environment variable for each environment.
const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  preferredLanguage: string;
  role: string;
  householdId: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
  user: UserDto;
}

export interface HouseholdDto {
  id: string;
  name: string;
  inviteCode: string;
  memberCount: number;
}

export interface MemberDto {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
  role: string;
  joinedAt: string;
  isCurrentUser: boolean;
}

export interface HouseholdDetailDto {
  id: string;
  name: string;
  inviteCode: string;
  inviteCodeExpiresAt: string;
  isInviteCodeExpired: boolean;
  members: MemberDto[];
}

export interface ChangeMemberRoleRequest {
  role: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  preferredLanguage?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshRequest {
  expiredAccessToken: string;
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  otpCode: string;
  newPassword: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface CreateHouseholdRequest {
  name: string;
}

export interface JoinHouseholdRequest {
  inviteCode: string;
}

export interface SocialAuthRequest {
  provider: 'Google' | 'Facebook' | 'Apple';
  providerToken: string;
  displayName?: string;
}

export const authApi = createApi({
  reducerPath: 'authApi',
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
  tagTypes: ['Household'],
  endpoints: (builder) => ({
    register: builder.mutation<AuthResponse, RegisterRequest>({
      query: (body) => ({ url: '/api/auth/register', method: 'POST', body }),
    }),
    login: builder.mutation<AuthResponse, LoginRequest>({
      query: (body) => ({ url: '/api/auth/login', method: 'POST', body }),
    }),
    refresh: builder.mutation<AuthResponse, RefreshRequest>({
      query: (body) => ({ url: '/api/auth/refresh', method: 'POST', body }),
    }),
    logout: builder.mutation<void, LogoutRequest>({
      query: (body) => ({ url: '/api/auth/logout', method: 'POST', body }),
    }),
    forgotPassword: builder.mutation<void, ForgotPasswordRequest>({
      query: (body) => ({ url: '/api/auth/forgot-password', method: 'POST', body }),
    }),
    resetPassword: builder.mutation<void, ResetPasswordRequest>({
      query: (body) => ({ url: '/api/auth/reset-password', method: 'POST', body }),
    }),
    createHousehold: builder.mutation<HouseholdDto, CreateHouseholdRequest>({
      query: (body) => ({ url: '/api/households', method: 'POST', body }),
      invalidatesTags: ['Household'],
    }),
    joinHousehold: builder.mutation<HouseholdDto, JoinHouseholdRequest>({
      query: (body) => ({ url: '/api/households/join', method: 'POST', body }),
      invalidatesTags: ['Household'],
    }),
    socialAuth: builder.mutation<AuthResponse, SocialAuthRequest>({
      query: (body) => ({ url: '/api/auth/social', method: 'POST', body }),
    }),
    getMyHousehold: builder.query<HouseholdDetailDto, void>({
      query: () => '/api/households',
      providesTags: ['Household'],
    }),
    removeMember: builder.mutation<void, { householdId: string; targetUserId: string }>({
      query: ({ householdId, targetUserId }) => ({
        url: `/api/households/${householdId}/members/${targetUserId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Household'],
    }),
    changeMemberRole: builder.mutation<void, { householdId: string; targetUserId: string; role: string }>({
      query: ({ householdId, targetUserId, role }) => ({
        url: `/api/households/${householdId}/members/${targetUserId}`,
        method: 'PATCH',
        body: { role } satisfies ChangeMemberRoleRequest,
      }),
      invalidatesTags: ['Household'],
    }),
    regenerateInviteCode: builder.mutation<HouseholdDto, { householdId: string }>({
      query: ({ householdId }) => ({
        url: `/api/households/${householdId}/invite-code/regenerate`,
        method: 'POST',
      }),
      invalidatesTags: ['Household'],
    }),
  }),
});

export const {
  useRegisterMutation,
  useLoginMutation,
  useRefreshMutation,
  useLogoutMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useCreateHouseholdMutation,
  useJoinHouseholdMutation,
  useSocialAuthMutation,
  useGetMyHouseholdQuery,
  useRemoveMemberMutation,
  useChangeMemberRoleMutation,
  useRegenerateInviteCodeMutation,
} = authApi;
