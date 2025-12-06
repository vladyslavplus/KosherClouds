import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { reviewsApi, ReviewResponseDto, ReviewType, ReviewStatus } from '@/lib/api/reviews';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Pagination } from '@/shared/ui/Pagination';
import { ReviewCard } from './components/ReviewCard';
import { StarRating } from './components/StarRating';

export default function ReviewsPage() {
  const { t } = useTranslation();

  const [reviews, setReviews] = useState<ReviewResponseDto[]>([]);
  const [isFetching, setIsFetching] = useState(false);
  const [isInitialLoad, setIsInitialLoad] = useState(true);
  const [totalReviews, setTotalReviews] = useState(0);
  const [averageRating, setAverageRating] = useState(0);
  const [sortBy, setSortBy] = useState('createdAt desc');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [isMobile, setIsMobile] = useState(false);

  const pageSize = isMobile ? 5 : 8;

  const sortOptions: SelectOption[] = [
    { value: 'createdAt desc', label: t('reviews.sortNewest') },
    { value: 'createdAt asc', label: t('reviews.sortOldest') },
    { value: 'rating desc', label: t('reviews.sortHighestRating') },
    { value: 'rating asc', label: t('reviews.sortLowestRating') },
  ];

  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  useEffect(() => {
    loadOverallStatistics();
  }, []);

  useEffect(() => {
    loadReviews();
  }, [sortBy, currentPage, pageSize]);

  const loadOverallStatistics = async () => {
    try {
      const response = await reviewsApi.getReviews({
        reviewType: ReviewType.Order,
        status: ReviewStatus.Published,
        pageNumber: 1,
        pageSize: 9999,
      });

      setTotalReviews(response.pagination.totalCount);

      if (response.data.length > 0) {
        const avgRating =
          response.data.reduce((sum, review) => sum + review.rating, 0) /
          response.data.length;
        setAverageRating(avgRating);
      }
    } catch (error) {
      console.error('Error loading statistics:', error);
    }
  };

  const loadReviews = async () => {
    try {
      setIsFetching(true);

      const response = await reviewsApi.getReviews({
        reviewType: ReviewType.Order,
        status: ReviewStatus.Published,
        orderBy: sortBy,
        pageNumber: currentPage,
        pageSize: pageSize,
      });

      setReviews(response.data);
      setTotalPages(response.pagination.totalPages);
    } catch (error) {
      console.error('Error loading reviews:', error);
    } finally {
      setIsFetching(false);
      setIsInitialLoad(false);
    }
  };

  const handleSortChange = (value: string) => {
    setSortBy(value);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (isInitialLoad) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#F3F4F6] py-8">
      <div className="max-w-5xl mx-auto px-4">
        <h1 className="text-4xl font-playfair font-bold mb-4 text-[#1A1F3A]">
          {t('reviews.title')}
        </h1>

        <div className="flex items-center gap-3 mb-6">
          <StarRating rating={5} maxRating={1} size="lg" />
          <span className="text-2xl font-bold text-[#1A1F3A] font-sans tabular-nums">
            {averageRating.toFixed(1)}
          </span>
          <span className="text-lg text-gray-600">
            / 5.0
          </span>
          <span className="text-gray-500">
            ({totalReviews} {t('reviews.reviewsCount')})
          </span>
        </div>

        <div className="flex justify-start mb-6">
          <Select
            options={sortOptions}
            value={sortBy}
            onChange={handleSortChange}
            rounded="full"
            className="w-auto"
          />
        </div>

        {reviews.length === 0 ? (
          <div className="text-center py-12 text-gray-600">
            {t('reviews.noReviews')}
          </div>
        ) : (
          <>
            <div className="space-y-6 relative min-h-[200px]">
              {isFetching && (
                <div className="absolute inset-0 bg-white/50 flex items-center justify-center z-10 rounded-2xl transition-opacity duration-200">
                  <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-[#8B6914] border-t-transparent"></div>
                </div>
              )}
              
              {reviews.map((review) => (
                <ReviewCard key={review.id} review={review} />
              ))}
            </div>
            
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={handlePageChange}
            />
          </>
        )}
      </div>
    </div>
  );
}