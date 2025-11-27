import { apiClient } from './client';

export enum ProductCategory {
  Dish = 'Dish',
  Set = 'Set',
  Dessert = 'Dessert',
  Drink = 'Drink',
  Hookah = 'Hookah',
}

export enum HookahStrength {
  Light = 'Light',
  Medium = 'Medium',
  Strong = 'Strong',
}

export interface HookahDetailsDto {
  tobaccoFlavor: string;
  tobaccoFlavorUk?: string | null;
  strength: HookahStrength;
  bowlType?: string | null;
  bowlTypeUk?: string | null;
  additionalParams: Record<string, string>;
  additionalParamsUk?: Record<string, string> | null;
}

export interface ProductResponse {
  id: string;
  name: string;
  nameUk?: string | null;
  description: string;
  descriptionUk?: string | null;
  price: number;
  discountPrice?: number | null;
  isPromotional: boolean;
  category: ProductCategory;
  subCategory?: string | null;
  subCategoryUk?: string | null;
  isVegetarian: boolean;
  ingredients: string[];
  ingredientsUk?: string[];
  allergens: string[];
  allergensUk?: string[];
  photos: string[];
  isAvailable: boolean;
  rating: number;
  ratingCount: number;
  hookahDetails?: HookahDetailsDto | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface ProductCreateRequest {
  name: string;
  nameUk?: string | null;
  description: string;
  descriptionUk?: string | null;
  price: number;
  discountPrice?: number | null;
  isPromotional?: boolean;
  category: ProductCategory;
  subCategory?: string | null;
  subCategoryUk?: string | null;
  isVegetarian?: boolean;
  ingredients?: string[];
  ingredientsUk?: string[];
  allergens?: string[];
  allergensUk?: string[];
  photos?: string[];
  isAvailable?: boolean;
  hookahDetails?: HookahDetailsDto | null;
}

export interface ProductUpdateRequest {
  name?: string | null;
  nameUk?: string | null;
  description?: string | null;
  descriptionUk?: string | null;
  price?: number | null;
  discountPrice?: number | null;
  isPromotional?: boolean | null;
  category?: ProductCategory | null;
  subCategory?: string | null;
  subCategoryUk?: string | null;
  isVegetarian?: boolean | null;
  ingredients?: string[] | null;
  ingredientsUk?: string[] | null;
  allergens?: string[] | null;
  allergensUk?: string[] | null;
  photos?: string[] | null;
  isAvailable?: boolean | null;
  rating?: number | null;
  ratingCount?: number | null;
  hookahDetails?: HookahDetailsDto | null;
}

export interface ProductParameters {
  pageNumber?: number;
  pageSize?: number;
  name?: string;
  nameUk?: string;
  category?: string;
  subCategory?: string;
  subCategoryUk?: string;
  minPrice?: number;
  maxPrice?: number;
  isAvailable?: boolean;
  isVegetarian?: boolean;
  isPromotional?: boolean;
  createdAtFrom?: string;
  createdAtTo?: string;
  orderBy?: string;
}

export interface SubCategoryDto {
  value: string;
  label: string;
  labelUk: string;
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface PagedProductResponse {
  items: ProductResponse[];
  pagination: PaginationMetadata;
}

export interface UploadPhotoResponse {
  url: string;
}

export interface UploadMultiplePhotosResponse {
  urls: string[];
}

export interface ProductPhotosResponse {
  productId: string;
  productName: string;
  photos: string[];
  photoCount: number;
}

export interface AddPhotosResponse {
  message: string;
  addedPhotos: string[];
  totalPhotos: number;
}

export interface RemovePhotoResponse {
  message: string;
  remainingPhotos: number;
}

export interface ReplacePhotosResponse {
  message: string;
  newPhotos: string[];
  totalPhotos: number;
}

export const productsApi = {

  getProducts: async (params?: ProductParameters): Promise<PagedProductResponse> => {
    const response = await apiClient.get<ProductResponse[]>('/products', { params });
    
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
          pageSize: params?.pageSize || 10,
          currentPage: params?.pageNumber || 1,
          totalPages: 1,
          hasNext: false,
          hasPrevious: false,
        };
    }

    return {
      items: response.data,
      pagination,
    };
  },

  getSubCategories: async (category: ProductCategory): Promise<SubCategoryDto[]> => {
    const response = await apiClient.get<SubCategoryDto[]>('/products/subcategories', {
      params: { category }
    });
    return response.data;
  },

  getProductById: async (id: string): Promise<ProductResponse> => {
    const response = await apiClient.get<ProductResponse>(`/products/${id}`);
    return response.data;
  },

  createProduct: async (data: ProductCreateRequest): Promise<ProductResponse> => {
    const response = await apiClient.post<ProductResponse>('/products', data);
    return response.data;
  },

  updateProduct: async (id: string, data: ProductUpdateRequest): Promise<void> => {
    await apiClient.put(`/products/${id}`, data);
  },

  deleteProduct: async (id: string): Promise<void> => {
    await apiClient.delete(`/products/${id}`);
  },

  getProductPhotos: async (id: string): Promise<ProductPhotosResponse> => {
    const response = await apiClient.get<ProductPhotosResponse>(`/products/${id}/photos`);
    return response.data;
  },

  addProductPhotosFromFiles: async (id: string, files: File[]): Promise<AddPhotosResponse> => {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await apiClient.post<AddPhotosResponse>(
      `/products/${id}/photos`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  addProductPhotosFromUrls: async (id: string, urls: string[]): Promise<AddPhotosResponse> => {
    const formData = new FormData();
    urls.forEach((url) => {
      formData.append('urls', url);
    });

    const response = await apiClient.post<AddPhotosResponse>(
      `/products/${id}/photos`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  removeProductPhoto: async (id: string, photoUrl: string): Promise<RemovePhotoResponse> => {
    const response = await apiClient.delete<RemovePhotoResponse>(`/products/${id}/photos`, {
      params: { photoUrl },
    });
    return response.data;
  },

  replaceProductPhotosWithFiles: async (id: string, files: File[]): Promise<ReplacePhotosResponse> => {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await apiClient.put<ReplacePhotosResponse>(
      `/products/${id}/photos`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  replaceProductPhotosWithUrls: async (id: string, urls: string[]): Promise<ReplacePhotosResponse> => {
    const formData = new FormData();
    urls.forEach((url) => {
      formData.append('urls', url);
    });

    const response = await apiClient.put<ReplacePhotosResponse>(
      `/products/${id}/photos`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  uploadPhotoFromFile: async (file: File): Promise<UploadPhotoResponse> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<UploadPhotoResponse>(
      '/products/upload-photo',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  uploadPhotoFromUrl: async (url: string): Promise<UploadPhotoResponse> => {
    const formData = new FormData();
    formData.append('url', url);

    const response = await apiClient.post<UploadPhotoResponse>(
      '/products/upload-photo',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  uploadMultiplePhotosFromFiles: async (files: File[]): Promise<UploadMultiplePhotosResponse> => {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await apiClient.post<UploadMultiplePhotosResponse>(
      '/products/upload-photos',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  uploadMultiplePhotosFromUrls: async (urls: string[]): Promise<UploadMultiplePhotosResponse> => {
    const formData = new FormData();
    urls.forEach((url) => {
      formData.append('urls', url);
    });

    const response = await apiClient.post<UploadMultiplePhotosResponse>(
      '/products/upload-photos',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  deletePhoto: async (url: string): Promise<{ message: string }> => {
    const response = await apiClient.delete<{ message: string }>('/products/delete-photo', {
      params: { url },
    });
    return response.data;
  },
};