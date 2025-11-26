import { apiClient } from './client';

export interface OrderParameters {
  userId?: string;
  status?: 'Draft' | 'Pending' | 'Paid' | 'Completed' | 'Canceled';
  paymentType?: 'OnPickup' | 'Online';
  searchTerm?: string;
  minTotalAmount?: number;
  maxTotalAmount?: number;
  minOrderDate?: string;
  maxOrderDate?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productNameSnapshot: string;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
  createdAt: string;
  updatedAt: string;
}

export interface Order {
  id: string;
  userId: string;
  status: 'Draft' | 'Pending' | 'Paid' | 'Completed' | 'Canceled';
  totalAmount: number;
  notes?: string | null;
  paymentType: 'OnPickup' | 'Online';
  createdAt: string;
  updatedAt: string;
  items: OrderItem[];
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export const ordersApi = {
  getOrders: async (params: OrderParameters = {}): Promise<{ data: Order[]; pagination: PaginationMetadata }> => {
    const response = await apiClient.get<Order[]>('/orders', { params });
    
    const paginationHeader = response.headers['x-pagination'];
    const pagination = paginationHeader ? JSON.parse(paginationHeader) : null;

    return {
      data: response.data,
      pagination,
    };
  },

  getOrderById: async (orderId: string): Promise<Order> => {
    const response = await apiClient.get<Order>(`/orders/${orderId}`);
    return response.data;
  },
};