import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { productsApi, ProductResponse, ProductCategory, ProductCreateRequest, ProductUpdateRequest, HookahStrength } from '@/lib/api/products';
import { Button } from '@/shared/ui/Button';
import { Input } from '@/shared/ui/Input';
import { Select, SelectOption } from '@/shared/ui/Select';
import CloseIcon from '@/assets/icons/close.svg?react';

interface ProductFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  product: ProductResponse | null;
}

type PhotoUploadMethod = 'url' | 'file';

export function ProductFormModal({ isOpen, onClose, onSuccess, product }: ProductFormModalProps) {
  const { t } = useTranslation();
  const isEditing = !!product;

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [nameUk, setNameUk] = useState('');
  const [description, setDescription] = useState('');
  const [descriptionUk, setDescriptionUk] = useState('');
  const [price, setPrice] = useState('');
  const [discountPrice, setDiscountPrice] = useState('');
  const [isPromotional, setIsPromotional] = useState(false);
  const [category, setCategory] = useState<ProductCategory>(ProductCategory.Dish);
  const [subCategory, setSubCategory] = useState('');
  const [subCategoryUk, setSubCategoryUk] = useState('');
  const [isVegetarian, setIsVegetarian] = useState(false);
  const [ingredients, setIngredients] = useState('');
  const [ingredientsUk, setIngredientsUk] = useState('');
  const [allergens, setAllergens] = useState('');
  const [allergensUk, setAllergensUk] = useState('');
  const [isAvailable, setIsAvailable] = useState(true);

  const [photoUploadMethod, setPhotoUploadMethod] = useState<PhotoUploadMethod>('url');
  const [photoUrls, setPhotoUrls] = useState('');
  const [photoFiles, setPhotoFiles] = useState<File[]>([]);
  const [uploadingPhotos, setUploadingPhotos] = useState(false);
  const [uploadedPhotoUrls, setUploadedPhotoUrls] = useState<string[]>([]);

  const [tobaccoFlavor, setTobaccoFlavor] = useState('');
  const [tobaccoFlavorUk, setTobaccoFlavorUk] = useState('');
  const [strength, setStrength] = useState<HookahStrength>(HookahStrength.Medium);
  const [bowlType, setBowlType] = useState('');
  const [bowlTypeUk, setBowlTypeUk] = useState('');

  const categoryOptions: SelectOption[] = [
    { value: ProductCategory.Dish, label: t('admin.products.categories.dish') },
    { value: ProductCategory.Set, label: t('admin.products.categories.set') },
    { value: ProductCategory.Dessert, label: t('admin.products.categories.dessert') },
    { value: ProductCategory.Drink, label: t('admin.products.categories.drink') },
    { value: ProductCategory.Hookah, label: t('admin.products.categories.hookah') },
  ];

  const strengthOptions: SelectOption[] = [
    { value: HookahStrength.Light, label: t('admin.productForm.strengthLevels.light') },
    { value: HookahStrength.Medium, label: t('admin.productForm.strengthLevels.medium') },
    { value: HookahStrength.Strong, label: t('admin.productForm.strengthLevels.strong') },
  ];

  useEffect(() => {
    if (product) {
      setName(product.name);
      setNameUk(product.nameUk || '');
      setDescription(product.description);
      setDescriptionUk(product.descriptionUk || '');
      setPrice(product.price.toString());
      setDiscountPrice(product.discountPrice?.toString() || '');
      setIsPromotional(product.isPromotional);
      setCategory(product.category);
      setSubCategory(product.subCategory || '');
      setSubCategoryUk(product.subCategoryUk || '');
      setIsVegetarian(product.isVegetarian);
      setIngredients(product.ingredients.join(', '));
      setIngredientsUk(product.ingredientsUk?.join(', ') || '');
      setAllergens(product.allergens.join(', '));
      setAllergensUk(product.allergensUk?.join(', ') || '');
      setUploadedPhotoUrls(product.photos);
      setIsAvailable(product.isAvailable);

      if (product.hookahDetails) {
        setTobaccoFlavor(product.hookahDetails.tobaccoFlavor);
        setTobaccoFlavorUk(product.hookahDetails.tobaccoFlavorUk || '');
        setStrength(product.hookahDetails.strength);
        setBowlType(product.hookahDetails.bowlType || '');
        setBowlTypeUk(product.hookahDetails.bowlTypeUk || '');
      }
    } else {
      resetForm();
    }
  }, [product]);

  const resetForm = () => {
    setName('');
    setNameUk('');
    setDescription('');
    setDescriptionUk('');
    setPrice('');
    setDiscountPrice('');
    setIsPromotional(false);
    setCategory(ProductCategory.Dish);
    setSubCategory('');
    setSubCategoryUk('');
    setIsVegetarian(false);
    setIngredients('');
    setIngredientsUk('');
    setAllergens('');
    setAllergensUk('');
    setIsAvailable(true);
    setPhotoUploadMethod('url');
    setPhotoUrls('');
    setPhotoFiles([]);
    setUploadedPhotoUrls([]);
    setTobaccoFlavor('');
    setTobaccoFlavorUk('');
    setStrength(HookahStrength.Medium);
    setBowlType('');
    setBowlTypeUk('');
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setPhotoFiles(Array.from(e.target.files));
    }
  };

  const handleUploadPhotos = async () => {
    setUploadingPhotos(true);
    setError(null);
    try {
      let urls: string[] = [];

      if (photoUploadMethod === 'url' && photoUrls.trim()) {
        const urlArray = photoUrls.split(',').map(u => u.trim()).filter(Boolean);
        const response = await productsApi.uploadMultiplePhotosFromUrls(urlArray);
        urls = response.urls;
      } else if (photoUploadMethod === 'file' && photoFiles.length > 0) {
        const response = await productsApi.uploadMultiplePhotosFromFiles(photoFiles);
        urls = response.urls;
      }

      setUploadedPhotoUrls([...uploadedPhotoUrls, ...urls]);
      setPhotoUrls('');
      setPhotoFiles([]);
    } catch (err: any) {
      setError(err.response?.data?.message || t('admin.productForm.uploadError'));
    } finally {
      setUploadingPhotos(false);
    }
  };

  const handleRemovePhoto = (index: number) => {
    setUploadedPhotoUrls(uploadedPhotoUrls.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      if (uploadedPhotoUrls.length === 0) {
        setError(t('admin.productForm.photosRequired'));
        setLoading(false);
        return;
      }

      const baseData = {
        name,
        nameUk: nameUk || null,
        description,
        descriptionUk: descriptionUk || null,
        price: parseFloat(price),
        discountPrice: discountPrice ? parseFloat(discountPrice) : null,
        isPromotional,
        category,
        subCategory: subCategory || null,
        subCategoryUk: subCategoryUk || null,
        isVegetarian,
        ingredients: ingredients.split(',').map(i => i.trim()).filter(Boolean),
        ingredientsUk: ingredientsUk ? ingredientsUk.split(',').map(i => i.trim()).filter(Boolean) : [],
        allergens: allergens.split(',').map(a => a.trim()).filter(Boolean),
        allergensUk: allergensUk ? allergensUk.split(',').map(a => a.trim()).filter(Boolean) : [],
        photos: uploadedPhotoUrls,
        isAvailable,
      };

      if (category === ProductCategory.Hookah && tobaccoFlavor) {
        const hookahData = {
          ...baseData,
          hookahDetails: {
            tobaccoFlavor,
            tobaccoFlavorUk: tobaccoFlavorUk || null,
            strength,
            bowlType: bowlType || null,
            bowlTypeUk: bowlTypeUk || null,
            additionalParams: {},
            additionalParamsUk: null,
          },
        };

        if (isEditing && product) {
          await productsApi.updateProduct(product.id, hookahData as ProductUpdateRequest);
        } else {
          await productsApi.createProduct(hookahData as ProductCreateRequest);
        }
      } else {
        if (isEditing && product) {
          await productsApi.updateProduct(product.id, baseData as ProductUpdateRequest);
        } else {
          await productsApi.createProduct(baseData as ProductCreateRequest);
        }
      }

      onSuccess();
      handleClose();
    } catch (err: any) {
      setError(err.response?.data?.message || t('admin.productForm.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    resetForm();
    setError(null);
    onClose();
  };

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      handleClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm bg-black/50 p-4"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 z-10">
          <div className="flex items-center justify-between">
            <h2 className="text-2xl font-heading font-semibold">
              {isEditing ? t('admin.productForm.titleEdit') : t('admin.productForm.titleCreate')}
            </h2>
            <button
              onClick={handleClose}
              className="p-2 hover:bg-gray-100 rounded-full transition-colors"
            >
              <CloseIcon className="w-6 h-6 text-gray-600" />
            </button>
          </div>
        </div>

        <div className="p-6">
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-600">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={t('admin.productForm.nameEn')}
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                bordered
              />
              <Input
                label={t('admin.productForm.nameUk')}
                value={nameUk}
                onChange={(e) => setNameUk(e.target.value)}
                bordered
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.productForm.descriptionEn')}
                </label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                  rows={3}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.productForm.descriptionUk')}
                </label>
                <textarea
                  value={descriptionUk}
                  onChange={(e) => setDescriptionUk(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                  rows={3}
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={t('admin.productForm.price')}
                type="number"
                step="0.01"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                required
                bordered
              />
              <Input
                label={t('admin.productForm.discountPrice')}
                type="number"
                step="0.01"
                value={discountPrice}
                onChange={(e) => setDiscountPrice(e.target.value)}
                bordered
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.productForm.category')}
                </label>
                <Select
                  options={categoryOptions}
                  value={category}
                  onChange={(value) => setCategory(value as ProductCategory)}
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={t('admin.productForm.subCategoryEn')}
                value={subCategory}
                onChange={(e) => setSubCategory(e.target.value)}
                bordered
              />
              <Input
                label={t('admin.productForm.subCategoryUk')}
                value={subCategoryUk}
                onChange={(e) => setSubCategoryUk(e.target.value)}
                bordered
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={t('admin.productForm.ingredientsEn')}
                value={ingredients}
                onChange={(e) => setIngredients(e.target.value)}
                bordered
              />
              <Input
                label={t('admin.productForm.ingredientsUk')}
                value={ingredientsUk}
                onChange={(e) => setIngredientsUk(e.target.value)}
                bordered
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={t('admin.productForm.allergensEn')}
                value={allergens}
                onChange={(e) => setAllergens(e.target.value)}
                bordered
              />
              <Input
                label={t('admin.productForm.allergensUk')}
                value={allergensUk}
                onChange={(e) => setAllergensUk(e.target.value)}
                bordered
              />
            </div>

            <div className="border rounded-lg p-4 space-y-4">
              <h3 className="text-lg font-semibold">{t('admin.productForm.photos')}</h3>

              <div className="flex gap-4 items-center">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="photoMethod"
                    checked={photoUploadMethod === 'url'}
                    onChange={() => setPhotoUploadMethod('url')}
                    className="w-4 h-4"
                  />
                  <span className="text-sm font-medium">{t('admin.productForm.uploadByUrl')}</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="photoMethod"
                    checked={photoUploadMethod === 'file'}
                    onChange={() => setPhotoUploadMethod('file')}
                    className="w-4 h-4"
                  />
                  <span className="text-sm font-medium">{t('admin.productForm.uploadFiles')}</span>
                </label>
              </div>

              {photoUploadMethod === 'url' ? (
                <div className="flex gap-2">
                  <Input
                    placeholder={t('admin.productForm.photoUrlsPlaceholder')}
                    value={photoUrls}
                    onChange={(e) => setPhotoUrls(e.target.value)}
                    bordered
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleUploadPhotos}
                    isLoading={uploadingPhotos}
                    className="whitespace-nowrap"
                  >
                    {uploadingPhotos ? t('admin.productForm.uploading') : t('admin.productForm.uploadButton')}
                  </Button>
                </div>
              ) : (
                <div className="flex flex-col sm:flex-row gap-2">
                  <input
                    type="file"
                    multiple
                    accept="image/*"
                    onChange={handleFileSelect}
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent text-sm"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleUploadPhotos}
                    isLoading={uploadingPhotos}
                    disabled={photoFiles.length === 0}
                    className="whitespace-nowrap"
                  >
                    {uploadingPhotos ? t('admin.productForm.uploading') : t('admin.productForm.uploadButton')}
                  </Button>
                </div>
              )}

              {uploadedPhotoUrls.length > 0 ? (
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {uploadedPhotoUrls.map((url, index) => (
                    <div key={index} className="relative group">
                      <img
                        src={url}
                        alt={`Product ${index + 1}`}
                        className="w-full h-24 object-cover rounded border border-gray-200"
                      />
                      <button
                        type="button"
                        onClick={() => handleRemovePhoto(index)}
                        className="absolute top-1 right-1 p-1 bg-red-600 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                        title={t('admin.productForm.removePhoto')}
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500 text-sm">
                  {t('admin.productForm.noPhotos')}
                </div>
              )}
            </div>

            {category === ProductCategory.Hookah && (
              <div className="border-t pt-6 space-y-4">
                <h3 className="text-lg font-semibold">
                  {t('admin.productForm.hookahDetails')}
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label={t('admin.productForm.tobaccoFlavorEn')}
                    value={tobaccoFlavor}
                    onChange={(e) => setTobaccoFlavor(e.target.value)}
                    required
                    bordered
                  />
                  <Input
                    label={t('admin.productForm.tobaccoFlavorUk')}
                    value={tobaccoFlavorUk}
                    onChange={(e) => setTobaccoFlavorUk(e.target.value)}
                    bordered
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-2">
                    {t('admin.productForm.strength')}
                  </label>
                  <Select
                    options={strengthOptions}
                    value={strength}
                    onChange={(value) => setStrength(value as HookahStrength)}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label={t('admin.productForm.bowlTypeEn')}
                    value={bowlType}
                    onChange={(e) => setBowlType(e.target.value)}
                    bordered
                  />
                  <Input
                    label={t('admin.productForm.bowlTypeUk')}
                    value={bowlTypeUk}
                    onChange={(e) => setBowlTypeUk(e.target.value)}
                    bordered
                  />
                </div>
              </div>
            )}

            <div className="border rounded-lg p-4">
              <h3 className="text-sm font-semibold mb-3">{t('admin.productForm.options')}</h3>
              <div className="flex flex-wrap gap-6">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isPromotional}
                    onChange={(e) => setIsPromotional(e.target.checked)}
                    className="w-5 h-5 rounded border-gray-300 text-secondary focus:ring-secondary"
                  />
                  <span className="text-sm font-medium">{t('admin.productForm.promotional')}</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isVegetarian}
                    onChange={(e) => setIsVegetarian(e.target.checked)}
                    className="w-5 h-5 rounded border-gray-300 text-secondary focus:ring-secondary"
                  />
                  <span className="text-sm font-medium">{t('admin.productForm.vegetarian')}</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isAvailable}
                    onChange={(e) => setIsAvailable(e.target.checked)}
                    className="w-5 h-5 rounded border-gray-300 text-secondary focus:ring-secondary"
                  />
                  <span className="text-sm font-medium">{t('admin.productForm.available')}</span>
                </label>
              </div>
            </div>

            <div className="flex gap-4 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                fullWidth
                onClick={handleClose}
              >
                {t('admin.productForm.cancel')}
              </Button>
              <Button
                type="submit"
                variant="primary"
                fullWidth
                isLoading={loading}
              >
                {isEditing ? t('admin.productForm.save') : t('admin.productForm.create')}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}