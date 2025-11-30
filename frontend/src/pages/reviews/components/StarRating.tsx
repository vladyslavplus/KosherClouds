import StarIcon from '@/assets/icons/star_rating.svg?react';
import { cn } from '@/lib/utils/cn';

export interface StarRatingProps {
  rating: number;
  maxRating?: number;
  size?: 'sm' | 'md' | 'lg';
  showNumber?: boolean;
  className?: string;
}

export const StarRating = ({
  rating,
  maxRating = 5,
  size = 'md',
  showNumber = false,
  className,
}: StarRatingProps) => {
  const sizes = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
  };

  const stars = Array.from({ length: maxRating }, (_, index) => {
    const starValue = index + 1;
    const isFilled = starValue <= rating;

    return (
      <StarIcon
        key={index}
        className={cn(
          sizes[size],
          isFilled ? 'text-[#4F378A]' : 'text-white',
          'transition-colors'
        )}
      />
    );
  });

  return (
    <div className={cn('flex items-center gap-1', className)}>
      {stars}
      {showNumber && (
        <span className="ml-1 text-sm font-semibold text-gray-700">
          {rating.toFixed(1)}
        </span>
      )}
    </div>
  );
};