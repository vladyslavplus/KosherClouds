import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { productsApi, ProductResponse, ProductCategory, ProductParameters } from '@/lib/api/products';
import { Button } from '@/shared/ui/Button';
import { Input } from '@/shared/ui/Input';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Pagination } from '@/shared/ui/Pagination';
import { LazyImage } from '@/shared/components/LazyImage';
import { ProductFormModal } from './ProductFormModal';

export function ProductsManagementTab() {
    const { t, i18n } = useTranslation();
    const isUk = i18n.language === 'uk';

    const [products, setProducts] = useState<ProductResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [searchTerm, setSearchTerm] = useState('');
    const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState('createdAt desc');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState<ProductResponse | null>(null);
    const [deletingProduct, setDeletingProduct] = useState<string | null>(null);

    const pageSize = 10;

    const sortOptions: SelectOption[] = [
        { value: 'createdAt desc', label: t('admin.products.sort.newest') },
        { value: 'createdAt asc', label: t('admin.products.sort.oldest') },
        { value: 'name asc', label: t('admin.products.sort.nameAsc') },
        { value: 'name desc', label: t('admin.products.sort.nameDesc') },
        { value: 'price asc', label: t('admin.products.sort.priceAsc') },
        { value: 'price desc', label: t('admin.products.sort.priceDesc') },
    ];

    useEffect(() => {
        const handler = setTimeout(() => setDebouncedSearchTerm(searchTerm), 500);
        return () => clearTimeout(handler);
    }, [searchTerm]);

    useEffect(() => {
        fetchProducts();
    }, [debouncedSearchTerm, sortBy, currentPage]);

    useEffect(() => {
        setCurrentPage(1);
    }, [debouncedSearchTerm, sortBy]);

    const fetchProducts = async () => {
        setLoading(true);
        try {
            const params: ProductParameters = {
                pageNumber: currentPage,
                pageSize,
                orderBy: sortBy,
            };

            if (debouncedSearchTerm.trim()) {
                if (isUk) params.nameUk = debouncedSearchTerm.trim();
                else params.name = debouncedSearchTerm.trim();
            }

            const response = await productsApi.getProducts(params);
            setProducts(response.items);
            setTotalPages(response.pagination.totalPages);
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = () => {
        setEditingProduct(null);
        setIsModalOpen(true);
    };

    const handleEdit = (product: ProductResponse) => {
        setEditingProduct(product);
        setIsModalOpen(true);
    };

    const handleDelete = async (id: string) => {
        if (!confirm(t('admin.products.deleteConfirm'))) return;
        setDeletingProduct(id);
        try {
            await productsApi.deleteProduct(id);
            fetchProducts();
        } finally {
            setDeletingProduct(null);
        }
    };

    const getProductName = (p: ProductResponse) =>
        isUk && p.nameUk ? p.nameUk : p.name;

    const getCategoryLabel = (category: ProductCategory) => {
        const key = category.toLowerCase() as 'dish' | 'set' | 'dessert' | 'drink' | 'hookah';
        return t(`admin.products.categories.${key}`);
    };

    if (loading && products.length === 0) {
        return (
            <div className="text-center py-12">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6 w-full">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 w-full">
                <h3 className="text-xl md:text-2xl font-heading font-semibold">{t('admin.products.title')}</h3>
                <Button
                    variant="primary"
                    size="sm"
                    onClick={handleCreate}
                    className="whitespace-nowrap w-full sm:w-auto"
                    icon={
                        <svg className="w-5 h-5" fill="white" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" />
                        </svg>
                    }
                >
                    {t('admin.products.addProduct')}
                </Button>
            </div>

            <div className="flex flex-col sm:flex-row gap-4 w-full">
                <div className="flex-1 min-w-0">
                    <Input
                        type="text"
                        placeholder={t('admin.products.searchPlaceholder')}
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        bordered
                    />
                </div>
                <div className="w-full sm:w-auto sm:min-w-[250px]">
                    <Select options={sortOptions} value={sortBy} onChange={setSortBy} />
                </div>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden w-full">
                <div className="overflow-x-auto">
                    <table className="w-full table-fixed min-w-[640px]">
                        <colgroup>
                            <col className="w-16 sm:w-20 md:w-24" />
                            <col className="w-auto min-w-[140px]" />
                            <col className="hidden md:table-column w-28 lg:w-32" />
                            <col className="w-24 sm:w-28 md:w-32" />
                            <col className="hidden lg:table-column w-32 xl:w-36" />
                            <col className="w-20 sm:w-24 md:w-32 lg:w-36" />
                        </colgroup>

                        <thead className="bg-gray-50 border-b border-gray-200">
                            <tr>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.photo')}</th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.name')}</th>
                                <th className="hidden md:table-cell px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.category')}</th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.price')}</th>
                                <th className="hidden lg:table-cell px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.status')}</th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">{t('admin.products.table.actions')}</th>
                            </tr>
                        </thead>

                        <tbody className="bg-white divide-y divide-gray-200">
                            {products.map((product) => (
                                <tr key={product.id} className="hover:bg-gray-50">
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <LazyImage src={product.photos[0]} alt={product.name} className="w-10 h-10 sm:w-12 sm:h-12 md:w-16 md:h-16 object-cover rounded" />
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs sm:text-sm font-medium text-gray-900 truncate">{getProductName(product)}</div>
                                        {product.subCategory && (
                                            <div className="text-[10px] sm:text-xs text-gray-500 truncate">
                                                {isUk && product.subCategoryUk ? product.subCategoryUk : product.subCategory}
                                            </div>
                                        )}
                                        <div className="md:hidden text-[10px] sm:text-xs text-gray-500 mt-1 truncate">{getCategoryLabel(product.category)}</div>
                                    </td>
                                    <td className="hidden md:table-cell px-2 sm:px-3 md:px-4 py-3">
                                        <span className="text-xs lg:text-sm text-gray-900 truncate block">{getCategoryLabel(product.category)}</span>
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs sm:text-sm font-medium text-gray-900 tabular-nums">
                                            {product.discountPrice ? (
                                                <div className="flex flex-col">
                                                    <span className="line-through text-gray-400 text-[10px] sm:text-xs">{product.price}</span>
                                                    <span className="text-[#8B6914]">{product.discountPrice}</span>
                                                </div>
                                            ) : (
                                                <span>{product.price}</span>
                                            )}
                                        </div>
                                    </td>
                                    <td className="hidden lg:table-cell px-2 sm:px-3 md:px-4 py-3">
                                        <div className="flex flex-col gap-1">
                                            {product.isAvailable ? (
                                                <span className="inline-flex items-center justify-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800 whitespace-nowrap">{t('admin.products.status.available')}</span>
                                            ) : (
                                                <span className="inline-flex items-center justify-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800 whitespace-nowrap">{t('admin.products.status.unavailable')}</span>
                                            )}
                                            {product.isPromotional && (
                                                <span className="inline-flex items-center justify-center px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800 whitespace-nowrap">{t('admin.products.status.promotional')}</span>
                                            )}
                                        </div>
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="flex items-center justify-end gap-1">
                                            <button
                                                onClick={() => handleEdit(product)}
                                                className="p-1.5 sm:p-2 text-gray-600 hover:bg-gray-100 rounded border border-gray-300 transition-colors"
                                                title={t('admin.products.actions.edit')}
                                            >
                                                <svg className="w-3.5 h-3.5 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                                </svg>
                                            </button>

                                            <button
                                                onClick={() => handleDelete(product.id)}
                                                disabled={deletingProduct === product.id}
                                                className="p-1.5 sm:p-2 text-red-600 hover:bg-red-50 rounded border border-red-600 transition-colors disabled:opacity-50"
                                                title={t('admin.products.actions.delete')}
                                            >
                                                {deletingProduct === product.id ? (
                                                    <svg className="animate-spin w-3.5 h-3.5 sm:w-4 sm:h-4" fill="none" viewBox="0 0 24 24">
                                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                                    </svg>
                                                ) : (
                                                    <svg className="w-3.5 h-3.5 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                                    </svg>
                                                )}
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {products.length === 0 && !loading && (
                <div className="text-center py-12 text-gray-600">
                    {t('admin.products.noProducts')}
                </div>
            )}

            <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />

            <ProductFormModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} onSuccess={() => fetchProducts()} product={editingProduct} />
        </div>
    );
}