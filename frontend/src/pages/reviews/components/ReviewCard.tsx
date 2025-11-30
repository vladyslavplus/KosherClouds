import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ReviewResponseDto, ReviewType } from '@/lib/api/reviews';
import { ordersApi, OrderResponseDto } from '@/lib/api/orders';
import { StarRating } from './StarRating';
import { format } from 'date-fns';
import { uk, enUS } from 'date-fns/locale';

interface ReviewCardProps {
  review: ReviewResponseDto;
}

export const ReviewCard = ({ review }: ReviewCardProps) => {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';
  const locale = isUk ? uk : enUS;

  const [orderDetails, setOrderDetails] = useState<OrderResponseDto | null>(null);

  useEffect(() => {
    if (review.reviewType === ReviewType.Order) {
      loadOrderDetails();
    }
  }, [review.orderId]);

  const loadOrderDetails = async () => {
    try {
      const order = await ordersApi.getOrderById(review.orderId);
      setOrderDetails(order);
    } catch (error) {
      console.error('Error loading order details:', error);
    }
  };

  const formattedDate = format(new Date(review.createdAt), 'dd.MM.yyyy', { locale });

  const productName = review.reviewType === ReviewType.Product && review.productName
    ? (isUk && review.productNameUk ? review.productNameUk : review.productName)
    : null;

  const orderProductNames = orderDetails?.items.map(item => 
    isUk && item.productNameSnapshotUk 
      ? item.productNameSnapshotUk 
      : item.productNameSnapshot
  ) || [];

  return (
    <div className="bg-white rounded-2xl border-2 border-[#E5E7EB] p-6 hover:border-[#8B6914] transition-colors">
      <div className="flex justify-between items-start mb-3">
        <div>
          <h3 className="font-semibold text-lg text-[#1A1F3A]">
            {review.userName || t('reviews.anonymous')}
          </h3>
          <p className="text-sm text-gray-500">{formattedDate}</p>
        </div>
        <StarRating rating={review.rating} size="md" />
      </div>

      {review.comment && (
        <p className="text-gray-700 mb-4 leading-relaxed">
          {review.comment}
        </p>
      )}

      {productName && (
        <div className="mb-3 text-sm text-gray-600">
          <span className="font-semibold">{t('reviews.product')}:</span> {productName}
        </div>
      )}

      {review.reviewType === ReviewType.Order && orderProductNames.length > 0 && (
        <div className="pt-4 mt-4 border-t">
          <p className="text-sm text-gray-600">
            <span className="font-semibold">{t('reviews.order')}:</span>{' '}
            {orderProductNames.join(', ')}
          </p>
        </div>
      )}

      {review.isVerifiedPurchase && (
        <div className={review.reviewType === ReviewType.Order && orderProductNames.length > 0 ? "mt-3" : "mt-3 pt-3 border-t"}>
          <span className="inline-flex items-center gap-1 text-xs font-medium text-green-700 bg-green-50 px-2 py-1 rounded-full">
            ✓ {t('reviews.verifiedPurchase')}
          </span>
        </div>
      )}
    </div>
  );
};