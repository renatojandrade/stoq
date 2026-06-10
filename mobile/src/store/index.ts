import { configureStore, combineReducers } from '@reduxjs/toolkit';
import { persistStore, persistReducer, FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER } from 'redux-persist';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authApi } from './api/authApi';
import { catalogApi } from './api/catalogApi';
import { inventoryApi } from './api/inventoryApi';
import { shoppingApi } from './api/shoppingApi';
import { receiptsApi } from './api/receiptsApi';
import authReducer from './authSlice';

const inventoryPersistConfig = {
  key: 'inventoryApi',
  storage: AsyncStorage,
  whitelist: ['queries'],
};

const shoppingPersistConfig = {
  key: 'shoppingApi',
  storage: AsyncStorage,
  whitelist: ['queries'],
};

const rootReducer = combineReducers({
  auth: authReducer,
  [authApi.reducerPath]: authApi.reducer,
  [catalogApi.reducerPath]: catalogApi.reducer,
  [inventoryApi.reducerPath]: persistReducer(inventoryPersistConfig, inventoryApi.reducer),
  [shoppingApi.reducerPath]: persistReducer(shoppingPersistConfig, shoppingApi.reducer),
  [receiptsApi.reducerPath]: receiptsApi.reducer,
});

export const store = configureStore({
  reducer: rootReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    }).concat(
      authApi.middleware,
      catalogApi.middleware,
      inventoryApi.middleware,
      shoppingApi.middleware,
      receiptsApi.middleware,
    ),
});

export const persistor = persistStore(store);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
