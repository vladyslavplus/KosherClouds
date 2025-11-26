import { useState } from 'react';
import { Input } from '../ui/Input';

interface EditableFieldProps {
  label: string;
  value: string;
  type?: 'text' | 'email' | 'tel' | 'password';
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function EditableField({
  label,
  value,
  type = 'text',
  onChange,
  disabled = false,
}: EditableFieldProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [tempValue, setTempValue] = useState(value);

  const handleEdit = () => {
    setTempValue(value);
    setIsEditing(true);
  };

  const handleCancel = () => {
    setTempValue(value);
    setIsEditing(false);
  };

  const handleSave = () => {
    onChange(tempValue);
    setIsEditing(false);
  };

  return (
    <div className="relative">
      <Input
        label={label}
        type={type}
        value={isEditing ? tempValue : value}
        onChange={(e) => setTempValue(e.target.value)}
        onBlur={handleSave}
        disabled={!isEditing || disabled}
        rounded="full"
        className={cn(
          'pr-12!',
          !isEditing && !disabled && 'hover:bg-gray-50',
          isEditing && 'ring-2 ring-[#3A3DEF] ring-offset-2'
        )}
      />
      {!isEditing ? (
        <button
          onClick={handleEdit}
          disabled={disabled}
          className="absolute right-4 top-[42px] text-gray-400 hover:text-gray-600 transition-colors disabled:opacity-50 cursor-pointer"
          aria-label="Edit"
        >
          <img src={new URL('../../assets/icons/edit.svg', import.meta.url).href} alt="Edit" className="w-5 h-5" />
        </button>
      ) : (
        <button
          onClick={handleCancel}
          className="absolute right-4 top-[42px] text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
          aria-label="Cancel"
        >
          <img src={new URL('../../assets/icons/close.svg', import.meta.url).href} alt="Cancel" className="w-5 h-5" />
        </button>
      )}
    </div>
  );
}

function cn(...classes: (string | boolean | undefined)[]) {
  return classes.filter(Boolean).join(' ');
}