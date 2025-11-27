import { ProductResponse } from '@/lib/api/products';
import { useTranslation } from 'react-i18next';
import { LazyImage } from './LazyImage';
import PlusIcon from '@/assets/icons/plus.svg?react';
import { Button } from '@/shared/ui/Button';

interface ProductCardProps {
  product: ProductResponse;
  onDetailsClick: () => void;
}

export const ProductCard = ({ product, onDetailsClick }: ProductCardProps) => {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const name = isUk && product.nameUk ? product.nameUk : product.name;

  return (
    <div className="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow relative">
      <div 
        className="aspect-square bg-gray-200 cursor-pointer overflow-hidden relative"
        onClick={onDetailsClick}
      >
        <div className="w-full h-full backface-hidden transform-[translateZ(0)]">
          <LazyImage
            src={product.photos[0]}
            alt={name}
            className="w-full h-full object-cover hover:scale-105 transition-transform duration-300 will-change-transform"
          />
        </div>
      </div>
      
      <button
        onClick={(e) => {
          e.stopPropagation();
          console.log('Add to cart:', product.id);
        }}
        className="absolute top-3 right-3 w-12 h-12 bg-white rounded-full shadow-md flex items-center justify-center text-[#8B6914] hover:text-[#6d5210] hover:bg-gray-50 transition-all z-10"
        aria-label={t('menu.addToCart')}
      >
        <PlusIcon className="w-6 h-6" />
      </button>

      <div className="p-4">
        <h3 
          className="font-playfair text-lg mb-2 cursor-pointer hover:text-[#8B6914] transition-colors line-clamp-2"
          onClick={onDetailsClick}
        >
          {name}
        </h3>
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            <span className="text-xl font-semibold text-[#1A1F3A]">
              {product.price.toFixed(2)}
            </span>
            <span className="text-sm text-gray-600">
              {t('menu.uah')}
            </span>
          </div>
        </div>
        
        {product.rating > 0 && (
          <div className="mb-3 flex items-center gap-1 text-sm text-gray-600">
            <span>⭐</span>
            <span>{product.rating.toFixed(1)}</span>
            <span className="text-gray-400">({product.ratingCount})</span>
          </div>
        )}

        <Button
          variant="primary"
          size="sm"
          rounded="full"
          fullWidth
          onClick={(e) => {
            e.stopPropagation();
            onDetailsClick();
          }}
        >
          {t('menu.details')}
        </Button>
      </div>
    </div>
  );
};