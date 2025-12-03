import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderResponseDto } from '@/lib/api/orders';
import { reviewsApi, OrderToReviewDto } from '@/lib/api/reviews';
import { Button } from '@/shared/ui/Button';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Pagination } from '@/shared/ui/Pagination';
import { ReviewModal } from './ReviewModal';

export function OrdersTab() {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const [orders, setOrders] = useState<OrderResponseDto[]>([]);
  const [ordersToReview, setOrdersToReview] = useState<OrderToReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [sortBy, setSortBy] = useState('createdAt desc');
  const [selectedOrderForReview, setSelectedOrderForReview] = useState<OrderToReviewDto | null>(null);

  const pageSize = 3;

  const sortOptions: SelectOption[] = [
    { value: 'createdAt desc', label: t('orders.sortBy.newest') },
    { value: 'createdAt asc', label: t('orders.sortBy.oldest') },
    { value: 'totalAmount desc', label: t('orders.sortBy.priceDesc') },
    { value: 'totalAmount asc', label: t('orders.sortBy.priceAsc') },
  ];

  useEffect(() => {
    fetchOrders();
    fetchOrdersToReview();
  }, [currentPage, sortBy]);

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const response = await ordersApi.getOrders({
        pageNumber: currentPage,
        pageSize,
        orderBy: sortBy,
      });
      setOrders(response.data);
      setTotalPages(response.pagination.totalPages);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const fetchOrdersToReview = async () => {
    try {
      const data = await reviewsApi.getMyOrdersToReview();
      setOrdersToReview(data);
    } catch (error) {
      console.error(error);
    }
  };

  const getOrderToReview = (orderId: string): OrderToReviewDto | undefined => {
    return ordersToReview.find(o => o.orderId === orderId);
  };

  const canReview = (orderId: string): boolean => {
    const orderToReview = getOrderToReview(orderId);
    if (!orderToReview) return false;
    
    const hasReviewsLeft = !orderToReview.orderReviewExists || orderToReview.reviewableProductsCount > 0;
    return orderToReview.daysLeftToReview > 0 && hasReviewsLeft;
  };

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    return date.toLocaleDateString(isUk ? 'uk-UA' : 'en-GB', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Paid':
      case 'Completed':
        return 'text-green-600 bg-green-50';
      case 'Pending':
        return 'text-yellow-600 bg-yellow-50';
      case 'Canceled':
        return 'text-red-600 bg-red-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  };

  const getPaymentTypeLabel = (paymentType: string): string => {
    return paymentType === 'Online' 
      ? t('checkout.paymentOnline') 
      : t('checkout.paymentOnPickup');
  };

  if (loading && orders.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">{t('common.loading')}</p>
      </div>
    );
  }

  if (!loading && orders.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">{t('orders.noOrders')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold">{t('profile.orders')}</h2>
        <div className="w-64">
          <Select
            options={sortOptions}
            value={sortBy}
            onChange={setSortBy}
            placeholder={t('orders.sortBy.label')}
          />
        </div>
      </div>

      <div className="space-y-4">
        {orders.map((order) => {
          const orderToReview = getOrderToReview(order.id);
          const reviewable = canReview(order.id);

          return (
            <div
              key={order.id}
              className="bg-white border border-gray-200 rounded-lg p-6 space-y-4"
            >
              <div className="flex items-start justify-between">
                <div className="space-y-1">
                  <p className="text-sm text-gray-500">
                    {t('orders.orderNumber')}: <span className="font-mono">{order.id.slice(0, 8)}</span>
                  </p>
                  <p className="text-sm text-gray-600">{formatDate(order.createdAt)}</p>
                  <p className="text-sm text-gray-600">
                    {t('checkout.paymentMethod')}: {getPaymentTypeLabel(order.paymentType)}
                  </p>
                </div>
                <div className="flex flex-col items-end gap-2">
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                    {t(`orders.status.${order.status.toLowerCase()}`)}
                  </span>
                  <p className="text-xl font-bold tabular-nums">
                    {order.totalAmount} {isUk ? 'грн' : 'uah'}
                  </p>
                </div>
              </div>

              <div className="border-t pt-4">
                <p className="text-sm font-medium mb-2">{t('orders.items')}:</p>
                <div className="space-y-1">
                  {order.items.map((item) => (
                    <div key={item.id} className="flex justify-between text-sm">
                      <span className="text-gray-700">
                        {isUk ? item.productNameSnapshotUk || item.productNameSnapshot : item.productNameSnapshot} x {item.quantity}
                      </span>
                      <span className="text-gray-600 tabular-nums">
                        {item.lineTotal} {isUk ? 'грн' : 'uah'}
                      </span>
                    </div>
                  ))}
                </div>
              </div>

              {reviewable && orderToReview && (
                <div className="border-t pt-4">
                  <div className="flex items-center justify-between">
                    <div className="text-sm">
                      {!orderToReview.orderReviewExists && (
                        <p className="text-gray-600 mb-1">{t('orders.canReviewOrder')}</p>
                      )}
                      {orderToReview.reviewableProductsCount > 0 && (
                        <p className="text-gray-600">
                          {t('orders.canReviewProducts', { count: orderToReview.reviewableProductsCount })}
                        </p>
                      )}
                      <p className="text-sm text-gray-500 mt-1">
                        {t('orders.daysLeft', { days: orderToReview.daysLeftToReview })}
                      </p>
                    </div>
                    <Button
                      variant="secondary"
                      size="sm"
                      onClick={() => setSelectedOrderForReview(orderToReview)}
                    >
                      {t('orders.leaveReview')}
                    </Button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>

      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={setCurrentPage}
      />

      {selectedOrderForReview && (
        <ReviewModal
          isOpen={!!selectedOrderForReview}
          onClose={() => setSelectedOrderForReview(null)}
          orderToReview={selectedOrderForReview}
          onReviewSubmitted={() => {
            setSelectedOrderForReview(null);
            fetchOrdersToReview();
          }}
        />
      )}
    </div>
  );
}