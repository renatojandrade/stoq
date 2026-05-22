import { configureStore } from '@reduxjs/toolkit';
import { authApi } from './api/authApi';
import { catalogApi } from './api/catalogApi';
import { inventoryApi } from './api/inventoryApi';
import { shoppingApi } from './api/shoppingApi';
import { receiptsApi } from './api/receiptsApi';
import authReducer from './authSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    [authApi.reducerPath]: authApi.reducer,
    [catalogApi.reducerPath]: catalogApi.reducer,
    [inventoryApi.reducerPath]: inventoryApi.reducer,
    [shoppingApi.reducerPath]: shoppingApi.reducer,
    [receiptsApi.reducerPath]: receiptsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      authApi.middleware,
      catalogApi.middleware,
      inventoryApi.middleware,
      shoppingApi.middleware,
      receiptsApi.middleware,
    ),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
