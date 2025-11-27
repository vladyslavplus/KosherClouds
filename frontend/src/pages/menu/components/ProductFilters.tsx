import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ProductCategory, productsApi, SubCategoryDto } from '@/lib/api/products';
import { Checkbox } from '@/shared/ui/Checkbox';

interface ProductFiltersProps {
  selectedCategory: ProductCategory | null;
  onCategoryChange: (category: ProductCategory | null) => void;
  isVegetarian: boolean;
  onVegetarianChange: (value: boolean) => void;
  isPromotional: boolean;
  onPromotionalChange: (value: boolean) => void;
  selectedSubcategory: string | null;
  onSubcategoryChange: (subcategory: string | null) => void;
}

export const ProductFilters = ({
  selectedCategory,
  onCategoryChange,
  isVegetarian,
  onVegetarianChange,
  isPromotional,
  onPromotionalChange,
  selectedSubcategory,
  onSubcategoryChange,
}: ProductFiltersProps) => {
  const { t, i18n } = useTranslation();
  const [dynamicSubcategories, setDynamicSubcategories] = useState<SubCategoryDto[]>([]);
  const isUk = i18n.language === 'uk';

  useEffect(() => {
    const fetchSubcategories = async () => {
      if (!selectedCategory) {
        setDynamicSubcategories([]);
        return;
      }
      try {
        const data = await productsApi.getSubCategories(selectedCategory);
        setDynamicSubcategories(data);
      } catch (error) {
        console.error(error);
        setDynamicSubcategories([]);
      }
    };

    fetchSubcategories();
  }, [selectedCategory]);

  const categories = [
    { value: ProductCategory.Dish, label: t('menu.dish') },
    { value: ProductCategory.Set, label: t('menu.set') },
    { value: ProductCategory.Dessert, label: t('menu.dessert') },
    { value: ProductCategory.Drink, label: t('menu.drink') },
    { value: ProductCategory.Hookah, label: t('menu.hookah') },
  ];

  return (
    <aside className="w-full lg:w-72 bg-[#262B4D] rounded-lg p-6 text-white shrink-0">
      <div className="mb-6">
        <h3 className="font-playfair text-xl mb-4">{t('menu.categories')}</h3>
        
        <Checkbox
          id="category-all"
          label={t('menu.allCategories')}
          checked={selectedCategory === null}
          onChange={() => onCategoryChange(null)}
          className="mb-3 hover:text-[#8B6914] transition-colors"
        />
        
        {categories.map((cat) => (
          <Checkbox
            key={cat.value}
            id={`category-${cat.value}`}
            label={cat.label}
            checked={selectedCategory === cat.value}
            onChange={() => onCategoryChange(cat.value)}
            className="mb-3 hover:text-[#8B6914] transition-colors"
          />
        ))}
      </div>

      <div className="mb-6">
        <h3 className="font-playfair text-xl mb-4">{t('menu.filters')}</h3>
        
        <Checkbox
          id="filter-vegetarian"
          label={t('menu.vegetarian')}
          checked={isVegetarian}
          onChange={(e) => onVegetarianChange(e.target.checked)}
          className="mb-3 hover:text-[#8B6914] transition-colors"
        />
        
        <Checkbox
          id="filter-promotional"
          label={t('menu.promotional')}
          checked={isPromotional}
          onChange={(e) => onPromotionalChange(e.target.checked)}
          className="mb-3 hover:text-[#8B6914] transition-colors"
        />
      </div>

      {selectedCategory && dynamicSubcategories.length > 0 && (
        <div>
          <h3 className="font-playfair text-xl mb-4">
            {selectedCategory === ProductCategory.Hookah ? t('menu.hookahType') : t('menu.subcategory')}
          </h3>
          
          {dynamicSubcategories.map((sub) => (
            <Checkbox
              key={sub.value}
              id={`subcategory-${sub.value}`}
              label={isUk ? sub.labelUk : sub.label}
              checked={selectedSubcategory === sub.value}
              onChange={() => onSubcategoryChange(selectedSubcategory === sub.value ? null : sub.value)}
              className="mb-3 hover:text-[#8B6914] transition-colors"
            />
          ))}
        </div>
      )}
    </aside>
  );
};