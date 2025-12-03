import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { reviewsApi, OrderToReviewDto, ReviewType } from '@/lib/api/reviews';
import { Button } from '@/shared/ui/Button';
import { StarRating } from '@/shared/components/StarRating';
import CloseIcon from '@/assets/icons/close.svg?react';

interface ReviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  orderToReview: OrderToReviewDto;
  onReviewSubmitted: () => void;
}

export function ReviewModal({ isOpen, onClose, orderToReview, onReviewSubmitted }: ReviewModalProps) {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const [activeTab, setActiveTab] = useState<'order' | 'products'>('order');
  const [orderRating, setOrderRating] = useState(5);
  const [orderComment, setOrderComment] = useState('');
  const [productReviews, setProductReviews] = useState<Map<string, { rating: number; comment: string }>>(new Map());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleOrderReviewSubmit = async () => {
    setLoading(true);
    setError(null);

    try {
      await reviewsApi.createReview({
        orderId: orderToReview.orderId,
        reviewType: ReviewType.Order,
        rating: orderRating,
        comment: orderComment.trim() || null,
      });

      onReviewSubmitted();
    } catch (err: any) {
      setError(err.response?.data?.message || t('reviews.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleProductReviewSubmit = async (productId: string) => {
    const review = productReviews.get(productId);
    if (!review) return;

    setLoading(true);
    setError(null);

    try {
      await reviewsApi.createReview({
        orderId: orderToReview.orderId,
        reviewType: ReviewType.Product,
        productId,
        rating: review.rating,
        comment: review.comment.trim() || null,
      });

      setProductReviews(prev => {
        const newMap = new Map(prev);
        newMap.delete(productId);
        return newMap;
      });

      onReviewSubmitted();
    } catch (err: any) {
      setError(err.response?.data?.message || t('reviews.error'));
    } finally {
      setLoading(false);
    }
  };

  const updateProductReview = (productId: string, field: 'rating' | 'comment', value: number | string) => {
    setProductReviews(prev => {
      const newMap = new Map(prev);
      const existing = newMap.get(productId) || { rating: 5, comment: '' };
      newMap.set(productId, { ...existing, [field]: value });
      return newMap;
    });
  };

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-semibold">{t('reviews.leaveReview')}</h2>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 rounded-full transition-colors"
            >
              <CloseIcon className="w-6 h-6 text-gray-600" />
            </button>
          </div>

          {error && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-600">
              {error}
            </div>
          )}

          <div className="flex gap-4 mb-6 border-b">
            {!orderToReview.orderReviewExists && (
              <button
                onClick={() => setActiveTab('order')}
                className={`pb-3 px-4 font-medium transition-colors ${
                  activeTab === 'order'
                    ? 'border-b-2 border-secondary text-secondary'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                {t('reviews.orderReview')}
              </button>
            )}
            {orderToReview.reviewableProductsCount > 0 && (
              <button
                onClick={() => setActiveTab('products')}
                className={`pb-3 px-4 font-medium transition-colors ${
                  activeTab === 'products'
                    ? 'border-b-2 border-secondary text-secondary'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                {t('reviews.productReviews')} ({orderToReview.reviewableProductsCount})
              </button>
            )}
          </div>

          {activeTab === 'order' && !orderToReview.orderReviewExists && (
            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium mb-2">{t('reviews.rating')}</label>
                <StarRating rating={orderRating} onRatingChange={setOrderRating} />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('reviews.comment')} ({t('booking.optional')})
                </label>
                <textarea
                  value={orderComment}
                  onChange={(e) => setOrderComment(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                  rows={4}
                  maxLength={1000}
                />
              </div>

              <div className="flex gap-4">
                <Button
                  type="button"
                  variant="outline"
                  fullWidth
                  onClick={onClose}
                >
                  {t('booking.cancel')}
                </Button>
                <Button
                  type="button"
                  variant="primary"
                  fullWidth
                  onClick={handleOrderReviewSubmit}
                  disabled={loading}
                >
                  {loading ? t('reviews.submitting') : t('reviews.submit')}
                </Button>
              </div>
            </div>
          )}

          {activeTab === 'products' && (
            <div className="space-y-6">
              {orderToReview.products
                .filter(p => !p.alreadyReviewed)
                .map((product) => {
                  const review = productReviews.get(product.productId) || { rating: 5, comment: '' };

                  return (
                    <div key={product.productId} className="border border-gray-200 rounded-lg p-4 space-y-4">
                      <div>
                        <p className="font-medium">
                          {isUk ? product.productNameUk || product.productName : product.productName}
                        </p>
                        <p className="text-sm text-gray-600">
                          {product.price} {isUk ? 'грн' : 'uah'} × {product.quantity}
                        </p>
                      </div>

                      <div>
                        <label className="block text-sm font-medium mb-2">{t('reviews.rating')}</label>
                        <StarRating 
                          rating={review.rating} 
                          onRatingChange={(rating) => updateProductReview(product.productId, 'rating', rating)} 
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium mb-2">
                          {t('reviews.comment')} ({t('booking.optional')})
                        </label>
                        <textarea
                          value={review.comment}
                          onChange={(e) => updateProductReview(product.productId, 'comment', e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                          rows={3}
                          maxLength={1000}
                        />
                      </div>

                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() => handleProductReviewSubmit(product.productId)}
                        disabled={loading}
                      >
                        {loading ? t('reviews.submitting') : t('reviews.submit')}
                      </Button>
                    </div>
                  );
                })}
            </div>
          )}

          {orderToReview.orderReviewExists && orderToReview.reviewableProductsCount === 0 && (
            <div className="text-center py-8">
              <p className="text-gray-600">{t('reviews.allReviewed')}</p>
              <Button
                type="button"
                variant="outline"
                className="mt-4"
                onClick={onClose}
              >
                {t('common.close')}
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}