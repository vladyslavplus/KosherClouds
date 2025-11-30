import { apiClient } from "./client";

export enum OrderStatus {
  Draft = "Draft",
  Pending = "Pending",
  Paid = "Paid",
  Completed = "Completed",
  Canceled = "Canceled",
}

export enum PaymentType {
  OnPickup = "OnPickup",
  Online = "Online",
}

export interface OrderParameters {
  userId?: string;
  status?: OrderStatus;
  paymentType?: PaymentType;
  searchTerm?: string;
  minTotalAmount?: number;
  maxTotalAmount?: number;
  minOrderDate?: string;
  maxOrderDate?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
}

export interface OrderItemResponseDto {
  id: string;
  productId: string;
  productNameSnapshot: string;
  productNameSnapshotUk?: string | null;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
  createdAt: string;
  updatedAt: string;
}

export interface OrderItemCreateDto {
  productId: string;
  productNameSnapshot: string;
  productNameSnapshotUk?: string | null;
  unitPriceSnapshot: number;
  quantity: number;
}

export interface OrderResponseDto {
  id: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  contactName: string;
  contactPhone: string;
  contactEmail: string;
  notes?: string | null;
  paymentType: PaymentType;
  createdAt: string;
  updatedAt: string;
  items: OrderItemResponseDto[];
}

export interface OrderCreateDto {
  userId: string;
  contactName: string;
  contactPhone: string;
  contactEmail: string;
  notes?: string | null;
  paymentType: PaymentType;
  items: OrderItemCreateDto[];
}

export interface OrderConfirmDto {
  contactName: string;
  contactPhone: string;
  notes?: string | null;
  paymentType: PaymentType;
}

export interface OrderUpdateDto {
  status?: OrderStatus | null;
  notes?: string | null;
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface PagedOrderResponse {
  data: OrderResponseDto[];
  pagination: PaginationMetadata;
}

export const ordersApi = {
  getOrders: async (
    params: OrderParameters = {}
  ): Promise<PagedOrderResponse> => {
    const response = await apiClient.get<OrderResponseDto[]>("/orders", {
      params,
    });

    const paginationHeader = response.headers["x-pagination"];
    let pagination: PaginationMetadata;

    if (paginationHeader) {
      const parsed = JSON.parse(paginationHeader);

      pagination = {
        totalCount: parsed.totalCount ?? parsed.TotalCount,
        pageSize: parsed.pageSize ?? parsed.PageSize,
        currentPage: parsed.currentPage ?? parsed.CurrentPage,
        totalPages: parsed.totalPages ?? parsed.TotalPages,
        hasNext: parsed.hasNext ?? parsed.HasNext,
        hasPrevious: parsed.hasPrevious ?? parsed.HasPrevious,
      };
    } else {
      pagination = {
        totalCount: response.data.length,
        pageSize: params.pageSize || 10,
        currentPage: params.pageNumber || 1,
        totalPages: 1,
        hasNext: false,
        hasPrevious: false,
      };
    }

    return {
      data: response.data,
      pagination,
    };
  },

  getOrderById: async (orderId: string): Promise<OrderResponseDto> => {
    const response = await apiClient.get<OrderResponseDto>(
      `/orders/${orderId}`
    );
    return response.data;
  },

  createOrderFromCart: async (): Promise<OrderResponseDto> => {
    const response = await apiClient.post<OrderResponseDto>(
      "/orders/from-cart"
    );
    return response.data;
  },

  confirmOrder: async (
    orderId: string,
    data: OrderConfirmDto
  ): Promise<OrderResponseDto> => {
    const response = await apiClient.put<OrderResponseDto>(
      `/orders/${orderId}/confirm`,
      data
    );
    return response.data;
  },

  updateOrder: async (orderId: string, data: OrderUpdateDto): Promise<void> => {
    await apiClient.put(`/orders/${orderId}`, data);
  },

  deleteOrder: async (orderId: string): Promise<void> => {
    await apiClient.delete(`/orders/${orderId}`);
  },

  deleteDraftOrder: async (orderId: string): Promise<void> => {
    await apiClient.delete(`/orders/${orderId}`);
  },
};

export const orderItemsApi = {
  getItemsByOrderId: async (
    orderId: string
  ): Promise<OrderItemResponseDto[]> => {
    const response = await apiClient.get<OrderItemResponseDto[]>(
      `/order-items/by-order/${orderId}`
    );
    return response.data;
  },

  getOrderItemById: async (itemId: string): Promise<OrderItemResponseDto> => {
    const response = await apiClient.get<OrderItemResponseDto>(
      `/order-items/${itemId}`
    );
    return response.data;
  },

  updateOrderItemQuantity: async (
    itemId: string,
    newQuantity: number
  ): Promise<void> => {
    await apiClient.put(`/order-items/${itemId}/quantity`, null, {
      params: { newQuantity },
    });
  },
};
