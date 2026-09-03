type MoochIconName =
  | "arrow-left"
  | "bell"
  | "calendar"
  | "check"
  | "chevron-right"
  | "clock"
  | "dog"
  | "grid"
  | "home"
  | "logout"
  | "map"
  | "medal"
  | "moon"
  | "note"
  | "pack"
  | "paw"
  | "plus"
  | "ranks"
  | "route"
  | "save"
  | "settings"
  | "trail"
  | "trash"
  | "trophy"
  | "upload"
  | "user"
  | "walk";

type MoochIconProps = {
  className?: string;
  name: MoochIconName;
};

export function MoochIcon({ className, name }: MoochIconProps) {
  const classes = className ? `mooch-icon ${className}` : "mooch-icon";

  switch (name) {
    case "arrow-left":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M19 12H6" />
          <path d="m12 6-6 6 6 6" />
        </svg>
      );
    case "bell":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M8 17h8" />
          <path d="M9 17V10a3 3 0 1 1 6 0v7" />
          <path d="M7 17h10l-1.2-1.7a2.2 2.2 0 0 1-.4-1.3V10a5.4 5.4 0 1 0-10.8 0v4a2.2 2.2 0 0 1-.4 1.3L7 17Z" />
        </svg>
      );
    case "calendar":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M7 4v3" />
          <path d="M17 4v3" />
          <path d="M5 8h14" />
          <path d="M6.5 6h11A2.5 2.5 0 0 1 20 8.5v9A2.5 2.5 0 0 1 17.5 20h-11A2.5 2.5 0 0 1 4 17.5v-9A2.5 2.5 0 0 1 6.5 6Z" />
        </svg>
      );
    case "check":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="m6 12.5 4 4L18 8" />
        </svg>
      );
    case "chevron-right":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="m10 7 5 5-5 5" />
        </svg>
      );
    case "clock":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="8" />
          <path d="M12 8v4l2.5 2" />
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
    case "grid":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M5 5h5v5H5Z" />
          <path d="M14 5h5v5h-5Z" />
          <path d="M5 14h5v5H5Z" />
          <path d="M14 14h5v5h-5Z" />
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
    case "logout":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M14 7h-4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h4" />
          <path d="m13 12 6 0" />
          <path d="m16 9 3 3-3 3" />
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
    case "note":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M6 4h10l2 2v14H6Z" />
          <path d="M15 4v4h3" />
          <path d="M9 12h6" />
          <path d="M9 16h4" />
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
    case "route":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="6" cy="6" r="2.4" />
          <circle cx="18" cy="18" r="2.4" />
          <path d="M8.2 6h4.6a3.2 3.2 0 0 1 0 6.4H11a3.2 3.2 0 0 0 0 6.4h4.8" />
        </svg>
      );
    case "save":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M5 5h12l2 2v12H5Z" />
          <path d="M8 5v5h7V5" />
          <path d="M8 19v-6h8v6" />
        </svg>
      );
    case "settings":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="3" />
          <path d="M19.4 15a1 1 0 0 0 .2 1.1l.1.1a1 1 0 0 1 0 1.4l-1 1a1 1 0 0 1-1.4 0l-.1-.1a1 1 0 0 0-1.1-.2 1 1 0 0 0-.6.9V20a1 1 0 0 1-1 1h-1.4a1 1 0 0 1-1-1v-.2a1 1 0 0 0-.7-.9 1 1 0 0 0-1 .2l-.2.1a1 1 0 0 1-1.4 0l-1-1a1 1 0 0 1 0-1.4l.1-.1a1 1 0 0 0 .2-1.1 1 1 0 0 0-.9-.6H4a1 1 0 0 1-1-1v-1.4a1 1 0 0 1 1-1h.2a1 1 0 0 0 .9-.7 1 1 0 0 0-.2-1l-.1-.2a1 1 0 0 1 0-1.4l1-1a1 1 0 0 1 1.4 0l.1.1a1 1 0 0 0 1.1.2 1 1 0 0 0 .6-.9V4a1 1 0 0 1 1-1h1.4a1 1 0 0 1 1 1v.2a1 1 0 0 0 .7.9 1 1 0 0 0 1-.2l.2-.1a1 1 0 0 1 1.4 0l1 1a1 1 0 0 1 0 1.4l-.1.1a1 1 0 0 0-.2 1.1 1 1 0 0 0 .9.6H20a1 1 0 0 1 1 1v1.4a1 1 0 0 1-1 1h-.2a1 1 0 0 0-.4 1.8" />
        </svg>
      );
    case "trail":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M12 4 8 9h2l-3 4h2l-3 5h12l-3-5h2l-3-4h2Z" />
          <path d="M12 18v2" />
        </svg>
      );
    case "trash":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M5 7h14" />
          <path d="M10 11v6" />
          <path d="M14 11v6" />
          <path d="M8 7l1-3h6l1 3" />
          <path d="M7 7l1 13h8l1-13" />
        </svg>
      );
    case "trophy":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M8 5h8v5a4 4 0 0 1-8 0Z" />
          <path d="M8 7H5v2a3 3 0 0 0 3 3" />
          <path d="M16 7h3v2a3 3 0 0 1-3 3" />
          <path d="M12 14v4" />
          <path d="M9 20h6" />
        </svg>
      );
    case "upload":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <path d="M12 16V5" />
          <path d="m8 9 4-4 4 4" />
          <path d="M5 16v3h14v-3" />
        </svg>
      );
    case "user":
      return (
        <svg aria-hidden="true" className={classes} viewBox="0 0 24 24">
          <circle cx="12" cy="8" r="3.5" />
          <path d="M5 19a7 7 0 0 1 14 0" />
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
