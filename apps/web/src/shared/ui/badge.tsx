import type { HTMLAttributes, ReactNode } from "react";

type BadgeVariant = "accent" | "gold" | "neutral" | "success";

type BadgeProps = HTMLAttributes<HTMLSpanElement> & {
  children: ReactNode;
  variant?: BadgeVariant;
};

export function Badge({
  children,
  className,
  variant = "gold",
  ...props
}: BadgeProps) {
  const classes = ["ui-badge", `ui-badge--${variant}`];

  if (className) {
    classes.push(className);
  }

  return (
    <span {...props} className={classes.join(" ")}>
      {children}
    </span>
  );
}
