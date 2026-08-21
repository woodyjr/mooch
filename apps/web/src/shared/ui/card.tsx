import type { ElementType, HTMLAttributes, ReactNode } from "react";

type CardVariant = "default" | "subtle";

type CardProps<T extends ElementType = "section"> = {
  as?: T;
  children: ReactNode;
  className?: string;
  variant?: CardVariant;
} & Omit<HTMLAttributes<HTMLElement>, "className" | "children">;

export function Card<T extends ElementType = "section">({
  as,
  children,
  className,
  variant = "default",
  ...props
}: CardProps<T>) {
  const Component = (as ?? "section") as ElementType;
  const classes = ["ui-card", `ui-card--${variant}`];

  if (className) {
    classes.push(className);
  }

  return (
    <Component {...props} className={classes.join(" ")}>
      {children}
    </Component>
  );
}
