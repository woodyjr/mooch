type MoochIconName =
  | "bell"
  | "dog"
  | "home"
  | "map"
  | "medal"
  | "moon"
  | "pack"
  | "paw"
  | "plus"
  | "ranks"
  | "trail"
  | "walk";

type MoochIconProps = {
  className?: string;
  name: MoochIconName;
};

export function MoochIcon({ className, name }: MoochIconProps) {
  const classes = className ? `mooch-icon ${className}` : "mooch-icon";

  switch (name) {
    case "bell":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M8 17h8" />
          <path d="M9 17V10a3 3 0 1 1 6 0v7" />
          <path d="M7 17h10l-1.2-1.7a2.2 2.2 0 0 1-.4-1.3V10a5.4 5.4 0 1 0-10.8 0v4a2.2 2.2 0 0 1-.4 1.3L7 17Z" />
        </svg>
      );
    case "dog":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M6 15v-3.3A3.7 3.7 0 0 1 9.7 8H14l2.8-2v4h1.5A1.7 1.7 0 0 1 20 11.7V15" />
          <path d="M8 15v3" />
          <path d="M16 15v3" />
          <path d="M12 12.5h.01" />
          <path d="M20 12c1.2.2 2 1 2 2.1 0 1-.7 1.8-1.8 2" />
          <path d="M6 15H4.8A1.8 1.8 0 0 1 3 13.2V11" />
        </svg>
      );
    case "home":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M5 11.2 12 6l7 5.2" />
          <path d="M7 10.8V18h10v-7.2" />
          <path d="M10 18v-4h4v4" />
        </svg>
      );
    case "map":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M4 7.5 9 5l6 2.5L20 5v11.5L15 19l-6-2.5L4 19Z" />
          <path d="M9 5v11.5" />
          <path d="M15 7.5V19" />
        </svg>
      );
    case "medal":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="m8 4 4 5 4-5" />
          <path d="m9.2 3.5 2.8 3.5 2.8-3.5" />
          <circle cx="12" cy="15" r="4" />
          <path d="m12 13.2.7 1.3 1.4.2-1 1 .2 1.5-1.3-.7-1.3.7.2-1.5-1-1 1.4-.2Z" />
        </svg>
      );
    case "moon":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M15.5 4.8a7.4 7.4 0 1 0 3.7 13.8 6.8 6.8 0 1 1-3.7-13.8Z" />
        </svg>
      );
    case "pack":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="8" cy="9" r="2.5" />
          <circle cx="16.5" cy="8.5" r="2" />
          <path d="M4.5 18a3.5 3.5 0 0 1 7 0" />
          <path d="M13 17.5a3 3 0 0 1 6 0" />
        </svg>
      );
    case "paw":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <ellipse cx="8" cy="8" rx="1.6" ry="2.4" />
          <ellipse cx="12" cy="6.6" rx="1.6" ry="2.5" />
          <ellipse cx="16" cy="8" rx="1.6" ry="2.4" />
          <path d="M8.2 17.2c0-2 1.7-3.7 3.8-3.7s3.8 1.7 3.8 3.7c0 1.3-1 2.3-2.3 2.3H10.5c-1.3 0-2.3-1-2.3-2.3Z" />
        </svg>
      );
    case "plus":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M12 6v12" />
          <path d="M6 12h12" />
        </svg>
      );
    case "ranks":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M7 18V9" />
          <path d="M12 18V6" />
          <path d="M17 18v-4" />
          <path d="M5 18h14" />
        </svg>
      );
    case "trail":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M12 4 8 9h2l-3 4h2l-3 5h12l-3-5h2l-3-4h2Z" />
          <path d="M12 18v2" />
        </svg>
      );
    case "walk":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="15.5" cy="5.5" r="1.8" />
          <path d="M12.5 10.2 15 8l2.8 1.6" />
          <path d="m8 18 2.8-4 1.2-3.8" />
          <path d="m12.2 18 1.6-4.2 2.7 2.2 2.2 2" />
          <path d="M9.7 11.2 7 13" />
        </svg>
      );
  }
}
