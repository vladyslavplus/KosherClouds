import { ProductResponse } from '@/lib/api/products';
import { useTranslation } from 'react-i18next';
import { useEffect } from 'react';
import { Button } from '@/shared/ui/Button';

interface ProductModalProps {
  product: ProductResponse;
  isOpen: boolean;
  onClose: () => void;
}

export const ProductModal = ({ product, isOpen, onClose }: ProductModalProps) => {
  const { t, i18n } = useTranslation();
  const lang = i18n.language === 'uk' ? 'Uk' : 'En';

  useEffect(() => {
    if (isOpen) {
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      document.body.style.paddingRight = `${scrollbarWidth}px`;
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
      document.body.style.paddingRight = '0px';
    }

    return () => {
      document.body.style.overflow = 'unset';
      document.body.style.paddingRight = '0px';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  const name = lang === 'Uk' && product.nameUk ? product.nameUk : product.name;
  const description = lang === 'Uk' && product.descriptionUk ? product.descriptionUk : product.description;
  const subCategory = lang === 'Uk' && product.subCategoryUk ? product.subCategoryUk : product.subCategory;
  const ingredients = lang === 'Uk' && product.ingredientsUk?.length ? product.ingredientsUk : product.ingredients;
  const allergens = lang === 'Uk' && product.allergensUk?.length ? product.allergensUk : product.allergens;

  const hasDiscount = product.discountPrice !== null && product.discountPrice !== undefined;
  const displayPrice = hasDiscount ? product.discountPrice : product.price;

  const hookahDetails = product.hookahDetails
    ? {
        tobaccoFlavor: lang === 'Uk' && product.hookahDetails.tobaccoFlavorUk
          ? product.hookahDetails.tobaccoFlavorUk
          : product.hookahDetails.tobaccoFlavor,
        bowlType: lang === 'Uk' && product.hookahDetails.bowlTypeUk
          ? product.hookahDetails.bowlTypeUk
          : product.hookahDetails.bowlType,
        strength: product.hookahDetails.strength,
        additionalParams: lang === 'Uk' && product.hookahDetails.additionalParamsUk
          ? product.hookahDetails.additionalParamsUk
          : product.hookahDetails.additionalParams,
      }
    : null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-lg max-w-lg md:max-w-xl lg:max-w-2xl w-full max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="relative">
          {product.photos[0] && (
            <img
              src={product.photos[0]}
              alt={name}
              className="w-full h-48 md:h-56 lg:h-64 object-cover"
            />
          )}
          
          {product.isPromotional && (
            <div className="absolute top-4 left-4">
              <span className="bg-linear-to-r from-[#8B6914] to-[#b8881a] text-white px-4 py-2 rounded-full text-base font-semibold shadow-lg">
                {t('menu.specialOffer')}
              </span>
            </div>
          )}
        </div>

        <div className="p-6">
          <div className="flex justify-between items-start mb-4">
            <h2 className="font-playfair text-2xl md:text-3xl text-[#1A1F3A]">{name}</h2>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 text-3xl leading-none -mt-1"
            >
              ×
            </button>
          </div>

          <div className="flex items-center gap-4 mb-4">
            {hasDiscount ? (
              <div className="flex items-center gap-3">
                <span className="text-gray-400 line-through text-xl">
                  {product.price.toFixed(2)} {t('menu.uah')}
                </span>
                <span className="text-2xl font-semibold text-[#8B6914]">
                  {displayPrice!.toFixed(2)} {t('menu.uah')}
                </span>
                <span className="bg-red-100 text-red-700 px-2 py-1 rounded text-sm font-semibold">
                  {t('menu.discount')} -{Math.round(((product.price - displayPrice!) / product.price) * 100)}%
                </span>
              </div>
            ) : (
              <span className="text-2xl font-semibold text-[#8B6914]">
                {product.price.toFixed(2)} {t('menu.uah')}
              </span>
            )}
            
            {product.rating > 0 && (
              <div className="flex items-center gap-1 text-gray-600">
                <span>⭐</span>
                <span>{product.rating.toFixed(1)}</span>
                <span className="text-sm text-gray-400">
                  ({product.ratingCount} {t('menu.reviews')})
                </span>
              </div>
            )}
          </div>

          <div className="mb-4">
            <h3 className="font-semibold text-lg mb-2">{t('menu.description')}</h3>
            <p className="text-gray-700">{description}</p>
          </div>

          {ingredients.length > 0 && (
            <div className="mb-4">
              <h3 className="font-semibold text-lg mb-2">{t('menu.ingredients')}</h3>
              <p className="text-gray-700">{ingredients.join(', ')}</p>
            </div>
          )}

          {allergens.length > 0 && (
            <div className="mb-4">
              <h3 className="font-semibold text-lg mb-2">{t('menu.allergens')}</h3>
              <p className="text-red-600">{allergens.join(', ')}</p>
            </div>
          )}

          {subCategory && (
            <div className="mb-4">
              <h3 className="font-semibold text-lg mb-2">{t('menu.subcategory')}</h3>
              <p className="text-gray-700">{subCategory}</p>
            </div>
          )}

          {hookahDetails && (
            <div className="mb-4">
              <h3 className="font-semibold text-lg mb-2">{t('menu.hookahDetails')}</h3>
              <div className="space-y-2 text-gray-700">
                <p>
                  <span className="font-medium">{t('menu.tobaccoFlavor')}:</span>{' '}
                  {hookahDetails.tobaccoFlavor}
                </p>
                <p>
                  <span className="font-medium">{t('menu.strength')}:</span>{' '}
                  {hookahDetails.strength
                    ? t(`menu.${hookahDetails.strength.toString().toLowerCase()}`)
                    : ''}
                </p>
                {hookahDetails.bowlType && (
                  <p>
                    <span className="font-medium">{t('menu.bowlType')}:</span>{' '}
                    {hookahDetails.bowlType}
                  </p>
                )}
                {hookahDetails.additionalParams &&
                  Object.entries(hookahDetails.additionalParams).map(([key, value]) => (
                    <p key={key}>
                      <span className="font-medium">{key}:</span> {value}
                    </p>
                  ))}
              </div>
            </div>
          )}

          <Button
            variant="primary"
            size="md"
            rounded="full"
            fullWidth
            onClick={onClose}
          >
            {t('menu.close')}
          </Button>
        </div>
      </div>
    </div>
  );
};