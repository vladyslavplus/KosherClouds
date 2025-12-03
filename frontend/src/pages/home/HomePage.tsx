import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { productsApi, ProductResponse } from '../../lib/api/products';
import { cartApi, ShoppingCartDto } from '../../lib/api/cart';
import PlusIcon from '@/assets/icons/plus.svg?react';
import PlusCartIcon from '@/assets/icons/plus_cart.svg?react';
import MinusIcon from '@/assets/icons/minus.svg?react';
import { useAuthStore } from '@/lib/stores/authStore';
import { Container } from '@/shared/layouts';
import { Button } from '@/shared/ui/Button';

function HomePage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [cart, setCart] = useState<ShoppingCartDto | null>(null);
  const [updatingProduct, setUpdatingProduct] = useState<string | null>(null);

  const isUk = i18n.language === 'uk';

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await productsApi.getProducts({
          category: 'Dish',
          pageSize: 3,
          isAvailable: true,
        });
        setProducts(response.items);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchProducts();
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      fetchCart();
    }
  }, [isAuthenticated]);

  const fetchCart = async () => {
    try {
      const cartData = await cartApi.getCart();
      setCart(cartData);
    } catch (error) {
      console.error(error);
    }
  };

  const getCartQuantity = (productId: string): number => {
    if (!cart) return 0;
    const item = cart.items.find((i) => i.productId === productId);
    return item?.quantity || 0;
  };

  const handleAddToCart = async (productId: string) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    const currentQuantity = getCartQuantity(productId);
    if (currentQuantity >= 40) return;

    setUpdatingProduct(productId);
    try {
      await cartApi.addOrUpdateItem({ productId, quantity: 1 });
      await fetchCart();
    } catch (error) {
      console.error(error);
    } finally {
      setUpdatingProduct(null);
    }
  };

  const handleRemoveFromCart = async (productId: string) => {
    if (!isAuthenticated) return;

    setUpdatingProduct(productId);
    try {
      await cartApi.addOrUpdateItem({ productId, quantity: -1 });
      await fetchCart();
    } catch (error) {
      console.error(error);
    } finally {
      setUpdatingProduct(null);
    }
  };

  if (loading) {
    return (
      <main className="grow py-8">
        <Container>
          <div className="text-center">{t('loading')}</div>
        </Container>
      </main>
    );
  }

  return (
    <main className="grow py-8">
      <Container>
        <div className="flex flex-col items-center text-center space-y-6">
          <h1 className="text-6xl font-['Poltawski_Nowy'] font-semibold text-primary">
            KOSHER CLOUDS
          </h1>
          
          <p className="text-xl text-gray-700">
            {t('home.subtitle')}
          </p>
          
          <Button
            variant="secondary"
            onClick={() => navigate('/menu')}
          >
            {t('home.menuButton')}
          </Button>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 w-full mt-12">
            {products.map((product) => {
              const cartQuantity = getCartQuantity(product.id);
              const isUpdating = updatingProduct === product.id;

              return (
                <div key={product.id} className="flex flex-col">
                  {product.photos.length > 0 && (
                    <img
                      src={product.photos[0]}
                      alt={isUk ? product.nameUk || product.name : product.name}
                      className="w-full h-48 object-cover rounded-lg mb-3"
                    />
                  )}
                  
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex-1 text-left">
                      <h3 className="text-lg font-medium mb-1">
                        {isUk ? product.nameUk || product.name : product.name}
                      </h3>
                      
                      <span className="text-base tabular-nums text-gray-700">
                        {product.discountPrice || product.price} {isUk ? 'грн' : 'uah'}
                      </span>
                    </div>
                    
                    <div className="flex h-12 items-center justify-end">
                      {isAuthenticated && (
                        <>
                          {cartQuantity === 0 ? (
                            <button
                              onClick={() => handleAddToCart(product.id)}
                              disabled={isUpdating}
                              className="p-2.5 rounded-full bg-secondary hover:bg-secondary/90 transition-colors shrink-0 disabled:opacity-50"
                            >
                              <PlusIcon className="w-7 h-7 text-white" />
                            </button>
                          ) : (
                            <div className="flex items-center gap-2 px-3 py-1.5 bg-white border-2 border-black rounded-full shrink-0">
                              <button
                                onClick={() => handleRemoveFromCart(product.id)}
                                disabled={isUpdating}
                                className="hover:text-[#8B6914] transition-colors disabled:opacity-50"
                              >
                                <MinusIcon className="w-4 h-4" />
                              </button>
                              <span className="font-semibold min-w-5 text-center -mt-0.5">
                                {cartQuantity}
                              </span>
                              <button
                                onClick={() => handleAddToCart(product.id)}
                                disabled={isUpdating || cartQuantity >= 40}
                                className="hover:text-[#8B6914] transition-colors disabled:opacity-50"
                              >
                                <PlusCartIcon className="w-4 h-4" />
                              </button>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </Container>
    </main>
  );
}

export default HomePage;