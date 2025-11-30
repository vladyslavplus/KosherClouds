import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { reviewsApi, ReviewType, OrderToReviewDto } from '@/lib/api/reviews';
import { Button } from '@/shared/ui/Button';
import StarIcon from '@/assets/icons/star_rating.svg?react';

interface ReviewState {
    orderId: string;
}

export default function ReviewPage() {
    const { t, i18n } = useTranslation();
    const isUk = i18n.language === 'uk';
    const navigate = useNavigate();
    const location = useLocation();
    const state = location.state as ReviewState | undefined;

    const [step, setStep] = useState<'order' | 'products' | 'completed'>('order');
    const [orderData, setOrderData] = useState<OrderToReviewDto | null>(null);
    const [loading, setLoading] = useState(true);

    const [orderRating, setOrderRating] = useState(5);
    const [orderComment, setOrderComment] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [productReviews, setProductReviews] = useState<Record<string, { rating: number; comment: string }>>({});

    useEffect(() => {
        if (!state?.orderId) {
            navigate('/');
            return;
        }

        const fetchOrderData = async () => {
            try {
                const response = await reviewsApi.getMyOrdersToReview();
                const order = response.find(o => o.orderId === state.orderId);

                if (order) {
                    setOrderData(order);
                    const initialReviews: Record<string, { rating: number; comment: string }> = {};
                    order.products.forEach(product => {
                        if (!product.alreadyReviewed) {
                            initialReviews[product.productId] = { rating: 5, comment: '' };
                        }
                    });
                    setProductReviews(initialReviews);
                }
            } catch (error) {
                console.error('Error fetching order data:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchOrderData();
    }, [state, navigate]);

    const handleSubmitOrderReview = async () => {
        if (!orderData) return;

        setSubmitting(true);
        try {
            await reviewsApi.createReview({
                orderId: orderData.orderId,
                reviewType: ReviewType.Order,
                rating: orderRating,
                comment: orderComment || undefined,
            });

            if (orderData.reviewableProductsCount > 0) {
                setStep('products');
            } else {
                setStep('completed');
            }
        } catch (error) {
            console.error('Error submitting order review:', error);
        } finally {
            setSubmitting(false);
        }
    };

    const handleSubmitProductReview = async (productId: string) => {
        if (!productReviews[productId] || !orderData) return;

        setSubmitting(true);
        try {
            await reviewsApi.createReview({
                orderId: orderData.orderId,
                reviewType: ReviewType.Product,
                productId,
                rating: productReviews[productId].rating,
                comment: productReviews[productId].comment || undefined,
            });

            const updated = { ...productReviews };
            delete updated[productId];
            setProductReviews(updated);

            if (Object.keys(updated).length === 0) {
                setStep('completed');
            }
        } catch (error) {
            console.error('Error submitting product review:', error);
        } finally {
            setSubmitting(false);
        }
    };

    const handleSkipProducts = () => {
        setStep('completed');
    };

    if (loading || !orderData) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#F3F4F6] py-12">
            <div className="max-w-2xl mx-auto px-4">
                {step === 'order' ? (
                    <div className="bg-white rounded-lg shadow-lg p-8">
                        <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
                            {t('review.leaveReview')}
                        </h1>
                        <p className="text-gray-600 mb-6">
                            {t('review.orderReview')}
                        </p>

                        <div className="mb-6">
                            <label className="block text-sm font-medium text-gray-700 mb-3">
                                {t('review.rating')}
                            </label>
                            <div className="flex gap-2">
                                {[1, 2, 3, 4, 5].map(star => (
                                    <button
                                        key={star}
                                        onClick={() => {
                                            const newRating = orderRating === star ? star - 1 : star;
                                            setOrderRating(newRating);
                                        }}
                                        className="cursor-pointer transition-opacity hover:opacity-70"
                                    >
                                        <StarIcon
                                            className={`w-6 h-6 transition-colors ${star <= orderRating ? 'text-[#4F378A]' : 'text-gray-300'
                                                }`}
                                        />
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div className="mb-6">
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                {t('review.comment')} ({t('review.optional')})
                            </label>
                            <textarea
                                value={orderComment}
                                onChange={(e) => setOrderComment(e.target.value)}
                                placeholder={t('review.commentPlaceholder')}
                                className="w-full h-32 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#8B6914] focus:border-transparent resize-none"
                            />
                        </div>

                        <div className="space-y-3">
                            <Button
                                onClick={handleSubmitOrderReview}
                                disabled={submitting}
                                className="w-full"
                            >
                                {submitting ? t('review.submitting') : t('review.submit')}
                            </Button>
                            <button
                                onClick={() => navigate('/profile', { state: { tab: 'orders' } })}
                                className="w-full text-gray-600 hover:text-gray-800 font-medium"
                            >
                                {t('review.skipReview')}
                            </button>
                        </div>
                    </div>
                ) : step === 'products' ? (
                    Object.keys(productReviews).length === 0 ? (
                        <div className="bg-white rounded-lg shadow-lg p-12 text-center">
                            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                </svg>
                            </div>
                            <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
                                {t('review.allReviewsSubmitted')}
                            </h1>
                            <p className="text-gray-600 mb-8">
                                {t('review.allReviewsSubmittedMessage')}
                            </p>
                            <Button
                                onClick={() => navigate('/profile', { state: { tab: 'orders' } })}
                                className="w-full"
                            >
                                {t('payment.success.viewOrders')}
                            </Button>
                        </div>
                    ) : (
                        <div className="bg-white rounded-lg shadow-lg">
                            <div className="p-8 border-b border-gray-200">
                                <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
                                    {t('review.productReviews')}
                                </h1>
                                <p className="text-lg text-gray-600">
                                    {t('review.rateProducts')}
                                </p>
                            </div>

                            <div className="p-8">
                                <div className="space-y-8">
                                    {Object.entries(productReviews).map(([productId], index, array) => {
                                        const product = orderData.products.find(p => p.productId === productId);
                                        if (!product) return null;

                                        const productName = isUk && product.productNameUk
                                            ? product.productNameUk
                                            : product.productName;

                                        return (
                                            <div
                                                key={productId}
                                                className={`${
                                                    index !== array.length - 1 ? 'pb-8 border-b border-gray-200' : ''
                                                }`}
                                            >
                                                <div className="flex justify-between items-start mb-6">
                                                    <h2 className="text-xl font-semibold text-[#1A1F3A]">
                                                        {productName}
                                                    </h2>
                                                    <Button
                                                        onClick={() => handleSubmitProductReview(productId)}
                                                        disabled={submitting}
                                                        size="sm"
                                                        variant="secondary"
                                                        rounded="lg"
                                                    >
                                                        {submitting ? t('review.submitting') : t('review.submitReview')}
                                                    </Button>
                                                </div>

                                                <div className="mb-6">
                                                    <label className="block text-base font-medium text-gray-700 mb-3">
                                                        {t('review.rating')}
                                                    </label>
                                                    <div className="flex gap-2">
                                                        {[1, 2, 3, 4, 5].map(star => (
                                                            <button
                                                                key={star}
                                                                onClick={() => {
                                                                    const currentRating = productReviews[productId]!.rating;
                                                                    const newRating = currentRating === star ? star - 1 : star;

                                                                    setProductReviews({
                                                                        ...productReviews,
                                                                        [productId]: {
                                                                            ...productReviews[productId]!,
                                                                            rating: newRating,
                                                                        },
                                                                    });
                                                                }}
                                                                className="cursor-pointer transition-opacity hover:opacity-70"
                                                            >
                                                                <StarIcon
                                                                    className={`w-6 h-6 transition-colors ${
                                                                        star <= (productReviews[productId]?.rating || 0)
                                                                            ? 'text-[#4F378A]'
                                                                            : 'text-gray-300'
                                                                    }`}
                                                                />
                                                            </button>
                                                        ))}
                                                    </div>
                                                </div>

                                                <div>
                                                    <label className="block text-base font-medium text-gray-700 mb-2">
                                                        {t('review.comment')} ({t('review.optional')})
                                                    </label>
                                                    <textarea
                                                        value={productReviews[productId]?.comment || ''}
                                                        onChange={(e) => {
                                                            setProductReviews({
                                                                ...productReviews,
                                                                [productId]: {
                                                                    ...productReviews[productId]!,
                                                                    comment: e.target.value,
                                                                },
                                                            });
                                                        }}
                                                        placeholder={t('review.commentPlaceholder')}
                                                        className="w-full h-28 px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#8B6914] focus:border-transparent resize-none text-base"
                                                    />
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>

                                <div className="mt-8 pt-8 border-t border-gray-200">
                                    <Button
                                        onClick={handleSkipProducts}
                                        variant="primary"
                                        className="w-full"
                                    >
                                        {t('review.finishReviewing')}
                                    </Button>
                                </div>
                            </div>
                        </div>
                    )
                ) : (
                    <div className="bg-white rounded-lg shadow-lg p-12 text-center">
                        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                        </div>
                        <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
                            {t('review.allReviewsSubmitted')}
                        </h1>
                        <p className="text-gray-600 mb-8">
                            {t('review.allReviewsSubmittedMessage')}
                        </p>
                        <Button
                            onClick={() => navigate('/profile', { state: { tab: 'orders' } })}
                            className="w-full"
                        >
                            {t('payment.success.viewOrders')}
                        </Button>
                    </div>
                )}
            </div>
        </div>
    );
}