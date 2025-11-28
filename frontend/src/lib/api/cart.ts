import { apiClient } from './client';

export interface CartItemAddDto {
  productId: string;
  quantity: number;
}

export interface ShoppingCartItemDto {
  productId: string;
  quantity: number;
}

export interface ShoppingCartDto {
  userId: string;
  items: ShoppingCartItemDto[];
}

export const cartApi = {
  getCart: async (): Promise<ShoppingCartDto> => {
    const { data } = await apiClient.get<ShoppingCartDto>('/Cart');
    return data;
  },

  addOrUpdateItem: async (dto: CartItemAddDto): Promise<ShoppingCartDto> => {
    const { data } = await apiClient.post<ShoppingCartDto>('/Cart/items', dto);
    return data;
  },

  removeItem: async (productId: string): Promise<void> => {
    await apiClient.delete(`/Cart/items/${productId}`);
  },

  clearCart: async (): Promise<void> => {
    await apiClient.delete('/Cart');
  },
};