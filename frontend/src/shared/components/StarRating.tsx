import StarIcon from '@/assets/icons/star_rating.svg?react';

interface StarRatingProps {
  rating: number;
  onRatingChange: (rating: number) => void;
  size?: 'sm' | 'md' | 'lg';
}

export function StarRating({ rating, onRatingChange, size = 'md' }: StarRatingProps) {
  const sizeClasses = {
    sm: 'w-5 h-5',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
  };

  return (
    <div className="flex gap-2">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onRatingChange(star)}
          className="cursor-pointer transition-opacity hover:opacity-70"
        >
          <StarIcon
            className={`${sizeClasses[size]} transition-colors ${
              star <= rating ? 'text-[#4F378A]' : 'text-gray-300'
            }`}
          />
        </button>
      ))}
    </div>
  );
}