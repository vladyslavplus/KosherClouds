import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { cartApi, ShoppingCartDto } from '@/lib/api/cart';
import { productsApi, ProductResponse } from '@/lib/api/products';
import { ordersApi } from '@/lib/api/orders';
import { useAuthStore } from '@/lib/stores/authStore';
import { CartItem } from './components/CartItem';
import { Button } from '@/shared/ui/Button';

export default function CartPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  
  const [cart, setCart] = useState<ShoppingCartDto | null>(null);
  const [products, setProducts] = useState<Map<string, ProductResponse>>(new Map());
  const [loading, setLoading] = useState(true);
  const [checkingOut, setCheckingOut] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    loadCart();
  }, [isAuthenticated, navigate]);

  const loadCart = async () => {
    if (!cart) {
      setLoading(true);
    }
    
    try {
      const cartData = await cartApi.getCart();
      
      const productPromises = cartData.items.map((item) =>
        productsApi.getProductById(item.productId)
      );
      const productsData = await Promise.all(productPromises);
      
      const productsMap = new Map<string, ProductResponse>();
      productsData.forEach((product) => {
        productsMap.set(product.id, product);
      });
      
      setProducts(productsMap);
      setCart(cartData);
    } catch (error) {
      console.error('Error loading cart:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCheckout = async () => {
    if (!cart || cart.items.length === 0) return;

    setCheckingOut(true);
    try {
      const order = await ordersApi.createOrderFromCart();
      
      navigate('/checkout', { state: { order } });
    } catch (error) {
      console.error('Error creating order:', error);
      alert(t('cart.checkoutError'));
    } finally {
      setCheckingOut(false);
    }
  };

  const totalAmount = cart?.items.reduce((sum, item) => {
    const product = products.get(item.productId);
    if (!product) return sum;
    const price = product.discountPrice ?? product.price;
    return sum + price * item.quantity;
  }, 0) ?? 0;

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="min-h-screen bg-[#F3F4F6] py-8">
        <div className="max-w-4xl mx-auto px-4">
          <h1 className="text-3xl font-playfair font-bold mb-8 text-[#1A1F3A]">
            {t('cart.title')}
          </h1>
          <div className="text-center py-12 text-gray-600">
            {t('cart.empty')}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#F3F4F6] py-8">
      <div className="max-w-4xl mx-auto px-4">
        <h1 className="text-3xl font-playfair font-bold mb-8 text-[#1A1F3A]">
          {t('cart.title')}
        </h1>

        <div className="space-y-4 mb-8">
          {cart.items.map((item) => {
            const product = products.get(item.productId);
            if (!product) return null;
            
            return (
              <CartItem
                key={item.productId}
                product={product}
                quantity={item.quantity}
                onUpdate={loadCart}
              />
            );
          })}
        </div>

        <div className="bg-white rounded-lg p-6 mb-6">
          <div className="flex justify-between items-center mb-4">
            <span className="text-xl font-semibold text-[#1A1F3A]">
              {t('cart.total')}
            </span>
            <span className="text-2xl font-bold text-[#8B6914] font-sans tabular-nums">
              {totalAmount.toFixed(2)} {t('menu.uah')}
            </span>
          </div>
        </div>

        <Button
          variant="secondary"
          size="md"
          rounded="full"
          fullWidth
          onClick={handleCheckout}
          isLoading={checkingOut}
        >
          {t('cart.checkout')}
        </Button>
      </div>
    </div>
  );
}