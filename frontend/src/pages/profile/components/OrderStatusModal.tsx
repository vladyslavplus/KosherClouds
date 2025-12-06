import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderResponseDto, OrderStatus } from '@/lib/api/orders';
import { Button } from '@/shared/ui/Button';
import { Select, SelectOption } from '@/shared/ui/Select';
import CloseIcon from '@/assets/icons/close.svg?react';

interface OrderStatusModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  order: OrderResponseDto | null;
}

export function OrderStatusModal({ isOpen, onClose, onSuccess, order }: OrderStatusModalProps) {
  const { t } = useTranslation();

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState<OrderStatus>(OrderStatus.Pending);

  const statusOptions: SelectOption[] = [
    { value: OrderStatus.Pending, label: t('admin.orders.status.pending') },
    { value: OrderStatus.Paid, label: t('admin.orders.status.paid') },
    { value: OrderStatus.Completed, label: t('admin.orders.status.completed') },
    { value: OrderStatus.Canceled, label: t('admin.orders.status.canceled') },
  ];

  useEffect(() => {
    if (order) {
      setStatus(order.status);
    }
  }, [order]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!order) return;

    setError(null);
    setLoading(true);

    try {
      await ordersApi.updateOrder(order.id, { status });
      setError(null);
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data?.message || t('admin.orderStatusModal.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setError(null);
    onClose();
  };

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      handleClose();
    }
  };

  if (!isOpen || !order) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm bg-black/50 p-4"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-sm w-full">
        <div className="border-b border-gray-200 px-5 py-3">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-heading font-semibold">
              {t('admin.orderStatusModal.title')}
            </h2>
            <button onClick={handleClose} className="p-1.5 hover:bg-gray-100 rounded-full transition-colors">
              <CloseIcon className="w-5 h-5 text-gray-600" />
            </button>
          </div>
        </div>

        <div className="p-5">
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-600 text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                {t('admin.orderStatusModal.status')}
              </label>
              <Select
                options={statusOptions}
                value={status}
                onChange={(value) => setStatus(value as OrderStatus)}
              />
            </div>

            <div className="flex gap-3 pt-3">
              <Button type="button" variant="outline" fullWidth onClick={handleClose} size="sm">
                {t('admin.orderStatusModal.cancel')}
              </Button>
              <Button type="submit" variant="primary" fullWidth isLoading={loading} size="sm">
                {t('admin.orderStatusModal.save')}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}