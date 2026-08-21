import type { ButtonHTMLAttributes, ReactNode } from "react";

type ButtonVariant = "primary" | "secondary" | "ghost" | "text" | "icon";
type ButtonSize = "sm" | "md" | "lg";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
  fullWidth?: boolean;
  variant?: ButtonVariant;
  size?: ButtonSize;
};

type ButtonClassOptions = {
  fullWidth?: boolean;
  size?: ButtonSize;
  variant?: ButtonVariant;
};

export function buttonClassName({
  fullWidth = false,
  size = "md",
  variant = "primary"
}: ButtonClassOptions = {}) {
  const classes = ["ui-button", `ui-button--${variant}`, `ui-button--${size}`];

  if (fullWidth) {
    classes.push("ui-button--full-width");
  }

  return classes.join(" ");
}

export function Button({
  children,
  className,
  fullWidth = false,
  size = "md",
  type = "button",
  variant = "primary",
  ...props
}: ButtonProps) {
  const classes = [buttonClassName({ fullWidth, size, variant })];

  if (className) {
    classes.push(className);
  }

  return (
    <button
      {...props}
      className={classes.join(" ")}
      type={type}
    >
      {children}
    </button>
  );
}
