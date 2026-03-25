import type { ReactNode } from "react";

export type RouteLayoutProps = {
  children: ReactNode;
  className?: string;
};

export default function RouteLayout({ children, className }: RouteLayoutProps) {
  return (
    <div className={`w-full px-4 py-6 sm:px-6 lg:px-8 lg:py-8 ${className ?? ""}`.trim()}>
      {children}
    </div>
  );
}
