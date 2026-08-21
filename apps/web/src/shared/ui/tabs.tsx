type TabItem<T extends string> = {
  label: string;
  value: T;
};

type TabsProps<T extends string> = {
  items: readonly TabItem<T>[];
  onChange: (value: T) => void;
  value: T;
};

export function Tabs<T extends string>({ items, onChange, value }: TabsProps<T>) {
  return (
    <div className="ui-tabs" role="tablist">
      {items.map((item) => {
        const isActive = item.value === value;

        return (
          <button
            aria-selected={isActive}
            className={isActive ? "ui-tabs__tab ui-tabs__tab--active" : "ui-tabs__tab"}
            key={item.value}
            onClick={() => onChange(item.value)}
            role="tab"
            type="button"
          >
            {item.label}
          </button>
        );
      })}
    </div>
  );
}
