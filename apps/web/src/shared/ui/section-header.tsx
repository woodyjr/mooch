import type { ReactNode } from "react";

type SectionHeaderProps = {
  action?: ReactNode;
  description?: string;
  eyebrow?: string;
  headingLevel?: "h1" | "h2" | "h3";
  title: string;
};

export function SectionHeader({
  action,
  description,
  eyebrow,
  headingLevel = "h1",
  title
}: SectionHeaderProps) {
  const HeadingTag = headingLevel;

  return (
    <div className="ui-section-header">
      <div className="ui-section-header__copy">
        {eyebrow && <p className="eyebrow">{eyebrow}</p>}
        <HeadingTag>{title}</HeadingTag>
        {description && <p>{description}</p>}
      </div>
      {action ? <div className="ui-section-header__action">{action}</div> : null}
    </div>
  );
}
