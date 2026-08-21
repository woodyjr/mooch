import type { ReactNode } from "react";

type EmptyStateProps = {
  action?: ReactNode;
  eyebrow?: string;
  message: string;
  title: string;
};

export function EmptyState({ action, eyebrow, message, title }: EmptyStateProps) {
  return (
    <div className="ui-empty-state">
      {eyebrow ? <p className="eyebrow">{eyebrow}</p> : null}
      <h2>{title}</h2>
      <p>{message}</p>
      {action ? <div className="ui-empty-state__action">{action}</div> : null}
    </div>
  );
}
