import type { InputHTMLAttributes, TextareaHTMLAttributes } from "react";

type BaseFieldProps = {
  error?: string;
  helper?: string;
  label: string;
};

type TextFieldProps = BaseFieldProps & InputHTMLAttributes<HTMLInputElement>;
type TextAreaFieldProps = BaseFieldProps & TextareaHTMLAttributes<HTMLTextAreaElement>;

export function TextField({
  error,
  helper,
  id,
  label,
  ...props
}: TextFieldProps) {
  return (
    <div className="ui-field">
      <label className="ui-field__label" htmlFor={id}>
        {label}
      </label>
      <input
        {...props}
        className="ui-field__input"
        id={id}
      />
      {helper && <p className="ui-field__helper">{helper}</p>}
      {error && <p className="ui-field__error">{error}</p>}
    </div>
  );
}

export function TextAreaField({
  error,
  helper,
  id,
  label,
  ...props
}: TextAreaFieldProps) {
  return (
    <div className="ui-field">
      <label className="ui-field__label" htmlFor={id}>
        {label}
      </label>
      <textarea
        {...props}
        className="ui-field__textarea"
        id={id}
      />
      {helper && <p className="ui-field__helper">{helper}</p>}
      {error && <p className="ui-field__error">{error}</p>}
    </div>
  );
}
