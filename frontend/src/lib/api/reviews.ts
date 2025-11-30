import { apiClient } from './client';

export enum ReviewType {
  Order = 'Order',
  Product = 'Product'
}

export enum ReviewStatus {
  Pending = 'Pending',
  Published = 'Published',
  Hidden = 'Hidden'
}

export interface ReviewParameters {
  productId?: string;
  userId?: string;
  orderId?: string;
  reviewType?: ReviewType;
  status?: ReviewStatus;
  minRating?: number;
  maxRating?: number;
  minCreatedDate?: string;
  maxCreatedDate?: string;
  verifiedOnly?: boolean;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
}

export interface OrderItemDto {
  id: string;
  productId: string;
  productNameSnapshot: string;
  productNameSnapshotUk?: string | null;
  unitPriceSnapshot: number;
  quantity: number;
}

export interface OrderDto {
  id: string;
  userId: string;
  status: string;
  totalAmount: number;
  createdAt: string;
  items: OrderItemDto[];
}

export interface UserDto {
  id: string;
  userName?: string | null;
  email?: string | null;
  firstName?: string | null;
  lastName?: string | null;
}

export interface ReviewableProductDto {
  productId: string;
  productName: string;
  productNameUk?: string | null;
  price: number;
  quantity: number;
  alreadyReviewed: boolean;
  existingReviewId?: string | null;
}

export interface OrderToReviewDto {
  orderId: string;
  orderDate: string;
  totalAmount: number;
  orderReviewExists: boolean;
  orderReviewId?: string | null;
  products: ReviewableProductDto[];
  reviewableProductsCount: number;
  daysLeftToReview: number;
}

export interface ReviewResponseDto {
  id: string;
  orderId: string;
  reviewType: ReviewType;
  productId?: string | null;
  productName?: string | null;
  productNameUk?: string | null;
  userId: string;
  userName?: string | null;
  rating: number;
  comment?: string | null;
  isVerifiedPurchase: boolean;
  status: string;
  createdAt: string;
  updatedAt?: string | null;
  moderationNotes?: string | null;
  moderatedBy?: string | null;
  moderatedAt?: string | null;
}

export interface ReviewCreateDto {
  orderId: string;
  reviewType: ReviewType;
  productId?: string | null;
  rating: number;
  comment?: string | null;
}

export interface ReviewUpdateDto {
  rating?: number | null;
  comment?: string | null;
}

export interface ReviewModerationDto {
  action: 'Hide' | 'Publish';
  moderationNotes?: string | null;
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface PagedReviewResponse {
  data: ReviewResponseDto[];
  pagination: PaginationMetadata;
}

export const reviewsApi = {
  getReviews: async (params: ReviewParameters = {}): Promise<PagedReviewResponse> => {
    const response = await apiClient.get<ReviewResponseDto[]>('/reviews', { params });
    
    const paginationHeader = response.headers['x-pagination'];
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

  getReviewById: async (reviewId: string): Promise<ReviewResponseDto> => {
    const response = await apiClient.get<ReviewResponseDto>(`/reviews/${reviewId}`);
    return response.data;
  },

  getMyOrdersToReview: async (): Promise<OrderToReviewDto[]> => {
    const response = await apiClient.get<OrderToReviewDto[]>('/reviews/my-orders-to-review');
    return response.data;
  },

  getReviewableProducts: async (orderId: string): Promise<ReviewableProductDto[]> => {
    const response = await apiClient.get<ReviewableProductDto[]>(
      `/reviews/order/${orderId}/reviewable-products`
    );
    return response.data;
  },

  createReview: async (data: ReviewCreateDto): Promise<ReviewResponseDto> => {
    const response = await apiClient.post<ReviewResponseDto>('/reviews', data);
    return response.data;
  },

  updateReview: async (reviewId: string, data: ReviewUpdateDto): Promise<ReviewResponseDto> => {
    const response = await apiClient.put<ReviewResponseDto>(`/reviews/${reviewId}`, data);
    return response.data;
  },

  softDeleteReview: async (reviewId: string): Promise<void> => {
    await apiClient.post(`/reviews/${reviewId}/soft-delete`);
  },

  deleteReview: async (reviewId: string): Promise<void> => {
    await apiClient.delete(`/reviews/${reviewId}`);
  },

  moderateReview: async (reviewId: string, data: ReviewModerationDto): Promise<void> => {
    await apiClient.patch(`/reviews/${reviewId}/moderate`, data);
  },
};