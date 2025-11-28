import { useTranslation } from 'react-i18next';
import { ProductResponse } from '@/lib/api/products';
import { cartApi } from '@/lib/api/cart';
import { useState } from 'react';
import PlusCartIcon from '@/assets/icons/plus_cart.svg?react';
import MinusIcon from '@/assets/icons/minus.svg?react';
import TrashIcon from '@/assets/icons/trash.svg?react';

interface CartItemProps {
    product: ProductResponse;
    quantity: number;
    onUpdate: () => void;
}

export const CartItem = ({ product, quantity, onUpdate }: CartItemProps) => {
    const { t, i18n } = useTranslation();
    const isUk = i18n.language === 'uk';
    const [isUpdating, setIsUpdating] = useState(false);

    const name = isUk && product.nameUk ? product.nameUk : product.name;
    const hasDiscount = product.discountPrice !== null && product.discountPrice !== undefined;
    const displayPrice = hasDiscount ? product.discountPrice : product.price;
    const category = t(`menu.${product.category.toLowerCase()}`);

    const handleAdd = async () => {
        if (isUpdating || quantity >= 40) return;
        setIsUpdating(true);
        try {
            await cartApi.addOrUpdateItem({ productId: product.id, quantity: 1 });
            onUpdate();
        } catch (error) {
            console.error('Error adding item:', error);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleRemove = async () => {
        if (isUpdating) return;
        setIsUpdating(true);
        try {
            await cartApi.addOrUpdateItem({ productId: product.id, quantity: -1 });
            onUpdate();
        } catch (error) {
            console.error('Error removing item:', error);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleDelete = async () => {
        if (isUpdating) return;
        setIsUpdating(true);
        try {
            await cartApi.removeItem(product.id);
            onUpdate();
        } catch (error) {
            console.error('Error deleting item:', error);
        } finally {
            setIsUpdating(false);
        }
    };

    return (
        <div className="bg-[#262B4D] rounded-lg p-4 flex gap-4">
            <div className="w-20 h-20 md:w-24 md:h-24 shrink-0 rounded-lg overflow-hidden bg-gray-200">
                <img
                    src={product.photos[0]}
                    alt={name}
                    className="w-full h-full object-cover"
                />
            </div>

            <div className="flex-1 min-w-0 flex flex-col justify-between">
                <div>
                    <h3 className="text-base md:text-lg font-semibold text-white mb-1 truncate">
                        {name}
                    </h3>

                    <p className="text-xs md:text-sm text-gray-300 mb-1">{category}</p>

                    {product.category === 'Hookah' && product.hookahDetails && (
                        <div className="text-xs md:text-sm text-gray-400 mb-1">
                            <span>
                                {isUk && product.hookahDetails.tobaccoFlavorUk
                                    ? product.hookahDetails.tobaccoFlavorUk
                                    : product.hookahDetails.tobaccoFlavor}
                            </span>
                            <span className="mx-2">•</span>
                            <span>{product.hookahDetails.strength}</span>
                        </div>
                    )}
                </div>

                <div className="flex items-center gap-2 mt-1">
                    {hasDiscount ? (
                        <>
                            <span className="text-gray-400 line-through text-xs md:text-sm">
                                {product.price.toFixed(2)}
                            </span>
                            <span className="text-lg md:text-xl font-bold text-[#8B6914]">
                                {displayPrice!.toFixed(2)}
                            </span>
                        </>
                    ) : (
                        <span className="text-lg md:text-xl font-bold text-white">
                            {product.price.toFixed(2)}
                        </span>
                    )}
                    <span className="text-xs md:text-sm text-gray-400">{t('menu.uah')}</span>
                </div>
            </div>

            <div className="flex flex-col justify-between items-end gap-2 shrink-0">
                <button
                    onClick={handleDelete}
                    disabled={isUpdating}
                    className="p-1 text-red-400 hover:text-red-300 transition-colors"
                    aria-label={t('cart.remove')}
                >
                    <TrashIcon className="w-5 h-5" />
                </button>

                <div className="flex items-center gap-1.5 px-3 py-1.5 bg-white border-2 border-black rounded-full">
                    <button
                        onClick={handleRemove}
                        disabled={isUpdating}
                        className="hover:text-[#8B6914] transition-colors flex items-center justify-center"
                    >
                        <MinusIcon className="w-3.5 h-3.5" />
                    </button>
                    
                    <span className="font-semibold min-w-5 text-center text-sm md:text-base leading-none text-black">
                        {quantity}
                    </span>
                    
                    <button
                        onClick={handleAdd}
                        disabled={isUpdating || quantity >= 40}
                        className="hover:text-[#8B6914] transition-colors disabled:opacity-50 flex items-center justify-center"
                    >
                        <PlusCartIcon className="w-3.5 h-3.5" />
                    </button>
                </div>
            </div>
        </div>
    );
};