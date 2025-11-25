import { useState } from 'react';
import { Radio } from './Radio';
import { cn } from '@/lib/utils/cn';

export interface RadioOption {
  value: string;
  label: string;
}

export interface RadioGroupProps {
  name: string;
  options: RadioOption[];
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  className?: string;
  orientation?: 'horizontal' | 'vertical';
}

export function RadioGroup({
  name,
  options,
  value: controlledValue,
  defaultValue,
  onChange,
  className,
  orientation = 'horizontal',
}: RadioGroupProps) {
  const [internalValue, setInternalValue] = useState(defaultValue || options[0]?.value || '');
  
  const value = controlledValue !== undefined ? controlledValue : internalValue;

  const handleChange = (newValue: string) => {
    if (controlledValue === undefined) {
      setInternalValue(newValue);
    }
    onChange?.(newValue);
  };

  return (
    <div
      className={cn(
        'flex gap-6',
        orientation === 'vertical' ? 'flex-col' : 'flex-row flex-wrap',
        className
      )}
      role="radiogroup"
    >
      {options.map((option) => (
        <Radio
          key={option.value}
          id={`${name}-${option.value}`}
          name={name}
          label={option.label}
          value={option.value}
          checked={value === option.value}
          onChange={() => handleChange(option.value)}
        />
      ))}
    </div>
  );
}