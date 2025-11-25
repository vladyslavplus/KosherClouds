import { useState } from 'react';
import { cn } from '@/lib/utils/cn';

export interface CounterButtonProps {
  min?: number;
  max?: number;
  defaultValue?: number;
  value?: number;
  onChange?: (value: number) => void;
  className?: string;
}

export function CounterButton({
  min = 1,
  max = 99,
  defaultValue = 1,
  value: controlledValue,
  onChange,
  className,
}: CounterButtonProps) {
  const [internalValue, setInternalValue] = useState(defaultValue);
  
  const value = controlledValue !== undefined ? controlledValue : internalValue;

  const handleDecrement = () => {
    const newValue = Math.max(min, value - 1);
    if (controlledValue === undefined) {
      setInternalValue(newValue);
    }
    onChange?.(newValue);
  };

  const handleIncrement = () => {
    const newValue = Math.min(max, value + 1);
    if (controlledValue === undefined) {
      setInternalValue(newValue);
    }
    onChange?.(newValue);
  };

  return (
    <div
      className={cn(
        'inline-flex items-center border-2 border-[#000000] rounded-[45px] overflow-hidden bg-white',
        className
      )}
    >
      <button
        onClick={handleDecrement}
        disabled={value <= min}
        className={cn(
          'px-6 py-3 font-heading font-bold text-2xl transition-colors hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed'
        )}
        aria-label="Decrease"
      >
        −
      </button>

      <div className="flex items-center justify-center px-8 py-3 font-sans font-normal text-2xl w-20 tabular-nums leading-none">
        {value}
      </div>

      <button
        onClick={handleIncrement}
        disabled={value >= max}
        className={cn(
          'px-6 py-3 font-heading font-bold text-2xl transition-colors hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed'
        )}
        aria-label="Increase"
      >
        +
      </button>
    </div>
  );
}