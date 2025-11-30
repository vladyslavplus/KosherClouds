import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { productsApi, ProductCategory, ProductResponse, ProductParameters } from '@/lib/api/products';
import { cartApi, ShoppingCartDto } from '@/lib/api/cart';
import { useAuthStore } from '@/lib/stores/authStore';
import { ProductCard } from './components/ProductCard';
import { ProductFilters } from './components/ProductFilters';
import { ProductModal } from './components/ProductModal';
import { Pagination } from '@/shared/ui/Pagination';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Input } from '@/shared/ui/Input';

export default function MenuPage() {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [cart, setCart] = useState<ShoppingCartDto | null>(null);

  const [selectedCategory, setSelectedCategory] = useState<ProductCategory | null>(null);
  const [isVegetarian, setIsVegetarian] = useState(false);
  const [isPromotional, setIsPromotional] = useState(false);

  const [selectedSubcategory, setSelectedSubcategory] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState('category asc');
  
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const [viewportWidth, setViewportWidth] = useState(typeof window !== 'undefined' ? window.innerWidth : 1200);
  const pageSize = viewportWidth < 768 ? 6 : 9;

  const [selectedProduct, setSelectedProduct] = useState<ProductResponse | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const sortOptions: SelectOption[] = [
    { value: 'category asc', label: t('menu.sortBy') },
    { value: 'price asc', label: t('menu.priceAsc') },
    { value: 'price desc', label: t('menu.priceDesc') },
    { value: 'name asc', label: t('menu.nameAsc') },
    { value: 'name desc', label: t('menu.nameDesc') },
    { value: 'rating desc', label: t('menu.ratingDesc') },
    { value: 'rating asc', label: t('menu.ratingAsc') },
  ];

  useEffect(() => {
    const handleResize = () => {
      setViewportWidth(window.innerWidth);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 500);

    return () => {
      clearTimeout(handler);
    };
  }, [searchTerm]);

  useEffect(() => {
    fetchProducts();
  }, [selectedCategory, isVegetarian, isPromotional, selectedSubcategory, sortBy, debouncedSearchTerm, currentPage, pageSize, isUk]);

  useEffect(() => {
    setCurrentPage(1);
  }, [selectedCategory, isVegetarian, isPromotional, selectedSubcategory, sortBy, debouncedSearchTerm, pageSize]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchCart();
    }
  }, [isAuthenticated]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      setError(null);

      const params: ProductParameters = {
        pageNumber: currentPage,
        pageSize: pageSize,
      };

      if (selectedCategory) {
        params.category = selectedCategory;
      }

      if (isVegetarian) {
        params.isVegetarian = true;
      }

      if (isPromotional) {
        params.isPromotional = true;
      }

      if (debouncedSearchTerm.trim()) {
        if (isUk) {
          params.nameUk = debouncedSearchTerm.trim();
        } else {
          params.name = debouncedSearchTerm.trim();
        }
      }

      const effectiveSortBy = sortBy || 'category asc';
      if (isUk && effectiveSortBy.includes('name')) {
        params.orderBy = effectiveSortBy.replace('name', 'nameUk');
      } else {
        params.orderBy = effectiveSortBy;
      }

      if (selectedSubcategory) {
        params.subCategory = selectedSubcategory;
      }

      const response = await productsApi.getProducts(params);
      setProducts(response.items);
      setTotalPages(response.pagination.totalPages);

    } catch (err) {
      setError(t('menu.error'));
      console.error('Error fetching products:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchCart = async () => {
    try {
      const cartData = await cartApi.getCart();
      setCart(cartData);
    } catch (error) {
      console.error('Error fetching cart:', error);
    }
  };

  const getCartQuantity = (productId: string): number => {
    if (!cart) return 0;
    const item = cart.items.find((i) => i.productId === productId);
    return item?.quantity || 0;
  };

  const handleCategoryChange = (category: ProductCategory | null) => {
    setSelectedCategory(category);
    setSelectedSubcategory(null);
  };

  const handleSubcategoryChange = (subcategory: string | null) => {
    setSelectedSubcategory(subcategory);
  };

  const openProductModal = (product: ProductResponse) => {
    setSelectedProduct(product);
    setIsModalOpen(true);
  };

  const closeProductModal = () => {
    setIsModalOpen(false);
    setSelectedProduct(null);
  };

  return (
    <div className="min-h-screen bg-[#F3F4F6]">
      <div className="border-t-[7px] border-[#4A4F86]" />

      <div className="max-w-7xl mx-auto px-4 py-4">
        <div className="text-center">
          <p className="text-lg text-[#1A1F3A] font-medium">
            {t('menu.hookahInfo')}
          </p>
        </div>
      </div>

      <div className="border-t-[7px] border-[#4A4F86]" />

      <div className="max-w-7xl mx-auto px-4 py-8">
        
        <div className="flex flex-col lg:flex-row gap-6 items-start">
          <div className="w-full lg:w-[280px] shrink-0 lg:mt-[78px]">
            <ProductFilters
              selectedCategory={selectedCategory}
              onCategoryChange={handleCategoryChange}
              isVegetarian={isVegetarian}
              onVegetarianChange={setIsVegetarian}
              isPromotional={isPromotional}
              onPromotionalChange={setIsPromotional}
              selectedSubcategory={selectedSubcategory}
              onSubcategoryChange={handleSubcategoryChange}
            />
          </div>

          <div className="flex-1 w-full">
            <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
              <div className="w-full sm:w-[280px]">
                <Input
                  type="text"
                  placeholder={t('menu.searchPlaceholder')}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  rounded="full"
                  bordered
                />
              </div>
              
              <div className="w-full sm:w-auto">
                <Select
                  options={sortOptions}
                  value={sortBy}
                  onChange={setSortBy}
                  placeholder={t('menu.sortBy')}
                  rounded="full"
                  iconStroke="#8B6914"
                />
              </div>
            </div>

            {loading && (
              <div className="text-center py-12">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
                <p className="mt-4 text-gray-600">{t('menu.loading')}</p>
              </div>
            )}

            {error && (
              <div className="text-center py-12 text-red-600">
                {error}
              </div>
            )}

            {!loading && !error && products.length === 0 && (
              <div className="text-center py-12 text-gray-600">
                {t('menu.noProducts')}
              </div>
            )}

            {!loading && !error && products.length > 0 && (
              <>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                  {products.map((product) => (
                    <ProductCard
                      key={product.id}
                      product={product}
                      onDetailsClick={() => openProductModal(product)}
                      cartQuantity={getCartQuantity(product.id)}
                      onCartUpdate={fetchCart}
                    />
                  ))}
                </div>
                <Pagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  onPageChange={setCurrentPage}
                />
              </>
            )}
          </div>
        </div>
      </div>

      {selectedProduct && (
        <ProductModal
          product={selectedProduct}
          isOpen={isModalOpen}
          onClose={closeProductModal}
        />
      )}
    </div>
  );
}