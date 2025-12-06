import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ProductsManagementTab } from './ProductsManagementTab';
import { OrdersManagementTab } from './OrdersManagementTab';
import { BookingsManagementTab } from './BookingsManagementTab';

export function AdminTab() {
    const { t } = useTranslation();
    const [activeSubTab, setActiveSubTab] = useState('products');

    const subTabs = [
        { id: 'products', label: t('admin.tabs.products') },
        { id: 'orders', label: t('admin.tabs.orders') },
        { id: 'bookings', label: t('admin.tabs.bookings') },
    ];

    return (
        <div className="space-y-6 w-full max-w-full">
            <div className="border-b border-gray-200">
                <nav className="flex gap-4 md:gap-8 overflow-x-auto">
                    {subTabs.map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveSubTab(tab.id)}
                            className={`pb-4 font-heading font-semibold text-base md:text-lg transition-colors whitespace-nowrap ${activeSubTab === tab.id
                                ? 'text-[#1A1F3A] border-b-2 border-[#1A1F3A]'
                                : 'text-gray-400 hover:text-gray-600'
                                }`}
                        >
                            {tab.label}
                        </button>
                    ))}
                </nav>
            </div>

            <div>
                {activeSubTab === 'products' && <ProductsManagementTab />}
                {activeSubTab === 'orders' && <OrdersManagementTab />}
                {activeSubTab === 'bookings' && <BookingsManagementTab />}
            </div>
        </div>
    );
}