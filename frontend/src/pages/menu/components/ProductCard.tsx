import { ProductResponse } from '@/lib/api/products';
import { useTranslation } from 'react-i18next';
import { LazyImage } from '@/shared/components/LazyImage';
import PlusIcon from '@/assets/icons/plus.svg?react';
import PlusCartIcon from '@/assets/icons/plus_cart.svg?react';
import MinusIcon from '@/assets/icons/minus.svg?react';
import { Button } from '@/shared/ui/Button';
import { cartApi } from '@/lib/api/cart';
import { useState } from 'react';
import { useAuthStore } from '@/lib/stores/authStore';

interface ProductCardProps {
  product: ProductResponse;
  onDetailsClick: () => void;
  cartQuantity: number;
  onCartUpdate: () => void;
}

export const ProductCard = ({ product, onDetailsClick, cartQuantity, onCartUpdate }: ProductCardProps) => {
  const { t, i18n } = useTranslation();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isUk = i18n.language === 'uk';
  const name = isUk && product.nameUk ? product.nameUk : product.name;
  const hasDiscount = product.discountPrice !== null && product.discountPrice !== undefined;
  const displayPrice = hasDiscount ? product.discountPrice : product.price;

  const [isUpdating, setIsUpdating] = useState(false);

  const handleAddToCart = async () => {
    if (!isAuthenticated || isUpdating || cartQuantity >= 40) return;

    setIsUpdating(true);
    try {
      await cartApi.addOrUpdateItem({ productId: product.id, quantity: 1 });
      onCartUpdate();
    } catch (error) {
      console.error('Error adding to cart:', error);
    } finally {
      setIsUpdating(false);
    }
  };

  const handleRemoveFromCart = async () => {
    if (!isAuthenticated || isUpdating) return;

    setIsUpdating(true);
    try {
      await cartApi.addOrUpdateItem({ productId: product.id, quantity: -1 });
      onCartUpdate();
    } catch (error) {
      console.error('Error removing from cart:', error);
    } finally {
      setIsUpdating(false);
    }
  };

  return (
    <div className="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow relative">
      <div
        className="aspect-square bg-gray-200 cursor-pointer overflow-hidden relative"
        onClick={onDetailsClick}
      >
        {product.isPromotional && (
          <div className="absolute top-3 left-3 z-20 max-w-[65%]">
            <span className="bg-linear-to-r from-[#8B6914] to-[#b8881a] 
                 text-white px-3 py-1.5 rounded-full text-sm 
                 font-semibold shadow-md block truncate">
              {t('menu.specialOffer')}
            </span>
          </div>
        )}

        <div className="w-full h-full backface-hidden transform-[translateZ(0)]">
          <LazyImage
            src={product.photos[0]}
            alt={name}
            className="w-full h-full object-cover hover:scale-105 transition-transform duration-300 will-change-transform"
          />
        </div>
      </div>

      {cartQuantity === 0 ? (
        <button
          onClick={(e) => {
            e.stopPropagation();
            handleAddToCart();
          }}
          disabled={!isAuthenticated || isUpdating}
          className="absolute top-3 right-3 w-12 h-12 bg-white rounded-full shadow-md flex items-center justify-center text-[#8B6914] hover:text-[#6d5210] hover:bg-gray-50 transition-all z-10 disabled:opacity-50"
          aria-label={t('menu.addToCart')}
        >
          <PlusIcon className="w-6 h-6" />
        </button>
      ) : (
        <div className="absolute top-3 right-1 z-10">
          <div className="flex items-center gap-2 px-3 py-1.5 bg-white border-2 border-black rounded-full">
            <button
              onClick={(e) => {
                e.stopPropagation();
                handleRemoveFromCart();
              }}
              disabled={isUpdating}
              className="hover:text-[#8B6914] transition-colors"
            >
              <MinusIcon className="w-4 h-4" />
            </button>
            <span className="font-semibold min-w-5 text-center -mt-0.5">{cartQuantity}</span>
            <button
              onClick={(e) => {
                e.stopPropagation();
                handleAddToCart();
              }}
              disabled={isUpdating || cartQuantity >= 40}
              className="hover:text-[#8B6914] transition-colors disabled:opacity-50"
            >
              <PlusCartIcon className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}

      <div className="p-4">
        <h3
          className="font-playfair text-lg mb-2 cursor-pointer hover:text-[#8B6914] transition-colors line-clamp-2"
          onClick={onDetailsClick}
        >
          {name}
        </h3>

        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            {hasDiscount ? (
              <div className="flex items-center gap-2">
                <span className="text-gray-400 line-through text-base">
                  {product.price.toFixed(2)}
                </span>
                <span className="text-xl font-semibold text-[#8B6914]">
                  {displayPrice!.toFixed(2)}
                </span>
                <span className="text-sm text-gray-600">
                  {t('menu.uah')}
                </span>
              </div>
            ) : (
              <>
                <span className="text-xl font-semibold text-[#1A1F3A]">
                  {product.price.toFixed(2)}
                </span>
                <span className="text-sm text-gray-600">
                  {t('menu.uah')}
                </span>
              </>
            )}
          </div>
        </div>

        <div className="mb-3 flex items-center gap-1 text-sm text-gray-600">
          <span>⭐</span>
          <span>{product.rating > 0 ? product.rating.toFixed(1) : '0.0'}</span>
          <span className="text-gray-400">({product.ratingCount})</span>
        </div>

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