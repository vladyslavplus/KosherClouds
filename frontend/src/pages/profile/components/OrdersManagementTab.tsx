import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderResponseDto, OrderParameters, OrderStatus } from '@/lib/api/orders';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Pagination } from '@/shared/ui/Pagination';
import { OrderStatusModal } from './OrderStatusModal';

export function OrdersManagementTab() {
    const { t, i18n } = useTranslation();
    const isUk = i18n.language === 'uk';

    const [orders, setOrders] = useState<OrderResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [sortBy, setSortBy] = useState('createdAt desc');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingOrder, setEditingOrder] = useState<OrderResponseDto | null>(null);
    const [deletingOrder, setDeletingOrder] = useState<string | null>(null);

    const pageSize = 10;

    const sortOptions: SelectOption[] = [
        { value: 'createdAt desc', label: t('admin.orders.sort.newest') },
        { value: 'createdAt asc', label: t('admin.orders.sort.oldest') },
        { value: 'totalAmount desc', label: t('admin.orders.sort.priceDesc') },
        { value: 'totalAmount asc', label: t('admin.orders.sort.priceAsc') },
    ];

    useEffect(() => {
        fetchOrders();
    }, [sortBy, currentPage]);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const params: OrderParameters = {
                pageNumber: currentPage,
                pageSize,
                orderBy: sortBy,
            };

            const response = await ordersApi.getOrders(params);
            setOrders(response.data);
            setTotalPages(response.pagination.totalPages);
        } catch (error) {
            console.error('Error fetching orders:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleEditStatus = (order: OrderResponseDto) => {
        setEditingOrder(order);
        setIsModalOpen(true);
    };

    const handleDelete = async (orderId: string) => {
        if (!confirm(t('admin.orders.deleteConfirm'))) return;

        setDeletingOrder(orderId);
        try {
            await ordersApi.deleteOrder(orderId);
            fetchOrders();
        } catch (error) {
            console.error('Error deleting order:', error);
            alert(t('admin.orders.deleteError'));
        } finally {
            setDeletingOrder(null);
        }
    };

    const handleModalClose = () => {
        setIsModalOpen(false);
        setEditingOrder(null);
    };

    const handleModalSuccess = () => {
        setIsModalOpen(false);
        setEditingOrder(null);
        fetchOrders();
    };

    const getStatusLabel = (status: OrderStatus): string => {
        const statusKey = status.toLowerCase() as 'draft' | 'pending' | 'paid' | 'completed' | 'canceled';
        return t(`admin.orders.status.${statusKey}`);
    };

    const getStatusColor = (status: OrderStatus): string => {
        switch (status) {
            case OrderStatus.Draft:
                return 'bg-gray-100 text-gray-800';
            case OrderStatus.Pending:
                return 'bg-yellow-100 text-yellow-800';
            case OrderStatus.Paid:
                return 'bg-blue-100 text-blue-800';
            case OrderStatus.Completed:
                return 'bg-green-100 text-green-800';
            case OrderStatus.Canceled:
                return 'bg-red-100 text-red-800';
            default:
                return 'bg-gray-100 text-gray-800';
        }
    };

    const formatDate = (dateString: string): string => {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat(isUk ? 'uk-UA' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        }).format(date);
    };

    if (loading && orders.length === 0) {
        return (
            <div className="text-center py-12">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6 w-full">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 w-full">
                <h3 className="text-xl md:text-2xl font-heading font-semibold">{t('admin.orders.title')}</h3>
                <div className="w-full sm:w-auto sm:min-w-[250px]">
                    <Select options={sortOptions} value={sortBy} onChange={setSortBy} />
                </div>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden w-full">
                <div className="overflow-x-auto">
                    <table className="w-full table-fixed min-w-[640px]">
                        <colgroup>
                            <col className="w-24 sm:w-32 md:w-36" />
                            <col className="w-auto min-w-[140px]" />
                            <col className="hidden md:table-column w-28 lg:w-32" />
                            <col className="w-24 sm:w-28 md:w-32" />
                            <col className="hidden lg:table-column w-36 xl:w-40" />
                            <col className="w-20 sm:w-24 md:w-32 lg:w-36" />
                        </colgroup>

                        <thead className="bg-gray-50 border-b border-gray-200">
                            <tr>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.orderId')}
                                </th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.customer')}
                                </th>
                                <th className="hidden md:table-cell px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.status')}
                                </th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.total')}
                                </th>
                                <th className="hidden lg:table-cell px-2 sm:px-3 md:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.date')}
                                </th>
                                <th className="px-2 sm:px-3 md:px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    {t('admin.orders.table.actions')}
                                </th>
                            </tr>
                        </thead>

                        <tbody className="bg-white divide-y divide-gray-200">
                            {orders.map((order) => (
                                <tr key={order.id} className="hover:bg-gray-50">
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs font-medium text-gray-900 font-mono truncate">
                                            #{order.id.slice(0, 6)}
                                        </div>
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs sm:text-sm font-medium text-gray-900 truncate">
                                            {order.contactName}
                                        </div>
                                        <div className="text-[10px] sm:text-xs text-gray-500 truncate">
                                            {order.contactPhone}
                                        </div>
                                        <div className="md:hidden mt-1">
                                            <span className={`inline-flex items-center justify-center px-1.5 py-0.5 rounded-full text-[10px] font-medium whitespace-nowrap ${getStatusColor(order.status)}`}>
                                                {getStatusLabel(order.status)}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="hidden md:table-cell px-2 sm:px-3 md:px-4 py-3">
                                        <span className={`inline-flex items-center justify-center px-2 py-1 rounded-full text-xs font-medium whitespace-nowrap ${getStatusColor(order.status)}`}>
                                            {getStatusLabel(order.status)}
                                        </span>
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs sm:text-sm font-medium text-gray-900 tabular-nums">
                                            {order.totalAmount}
                                        </div>
                                        <div className="text-[10px] sm:text-xs text-gray-500 whitespace-nowrap">
                                            {order.items.length} {isUk ? 'шт' : 'pcs'}
                                        </div>
                                    </td>
                                    <td className="hidden lg:table-cell px-2 sm:px-3 md:px-4 py-3">
                                        <div className="text-xs xl:text-sm text-gray-900">{formatDate(order.createdAt)}</div>
                                    </td>
                                    <td className="px-2 sm:px-3 md:px-4 py-3">
                                        <div className="flex items-center justify-end gap-1">
                                            <button
                                                onClick={() => handleEditStatus(order)}
                                                className="p-1.5 sm:p-2 text-gray-600 hover:bg-gray-100 rounded border border-gray-300 transition-colors"
                                                title={t('admin.orders.actions.editStatus')}
                                            >
                                                <svg className="w-3.5 h-3.5 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                                </svg>
                                            </button>

                                            <button
                                                onClick={() => handleDelete(order.id)}
                                                disabled={deletingOrder === order.id}
                                                className="p-1.5 sm:p-2 text-red-600 hover:bg-red-50 rounded border border-red-600 transition-colors disabled:opacity-50"
                                                title={t('admin.orders.actions.delete')}
                                            >
                                                {deletingOrder === order.id ? (
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

            {orders.length === 0 && !loading && (
                <div className="text-center py-12 text-gray-600">
                    {t('admin.orders.noOrders')}
                </div>
            )}

            <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />

            <OrderStatusModal isOpen={isModalOpen} onClose={handleModalClose} onSuccess={handleModalSuccess} order={editingOrder} />
        </div>
    );
}