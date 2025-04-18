export type RouteLayoutProps = {
  children: React.ReactNode;
  className?: string;
}

export default function RouteLayotut({ children, className }: RouteLayoutProps) {
  return (
    <div className={`w-full bg-base-300 h-auto p-6 ${className ?? ''}`}>
      {children}
    </div>
  );
}
