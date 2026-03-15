import type { ReactNode } from "react";

interface PageContainerProps {
  children: ReactNode;
  sidebar?: ReactNode;
}

export function PageContainer({ children, sidebar }: PageContainerProps) {
  return (
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-4 px-4 py-4 sm:px-6 lg:flex-row lg:px-8 lg:py-6">
      {sidebar}
      <section className="min-h-[60vh] flex-1 rounded-xl border border-[color:var(--line)] bg-white p-4 shadow-sm sm:p-6">
        {children}
      </section>
    </div>
  );
}
