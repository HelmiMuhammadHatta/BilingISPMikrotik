"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { LayoutDashboard, Users, Clock, FileText, Activity, Wifi, X } from "lucide-react";
import { cn } from "@/lib/utils";

const navItems = [
  { name: "Overview", href: "/", icon: LayoutDashboard },
  { name: "Pending Invoices", href: "/pending", icon: Clock },
  { name: "Customers", href: "/customers", icon: Users },
  { name: "Service Plans", href: "/service-plans", icon: Activity },
  { name: "All Invoices", href: "/invoices", icon: FileText },
  { name: "Audit Logs", href: "/audit-logs", icon: Activity },
];

export function Sidebar({ onClose }: { onClose?: () => void }) {
  const pathname = usePathname();

  return (
    <div className="flex h-screen w-[260px] flex-col border-r border-border bg-surface">
      <div className="flex h-16 items-center justify-between px-6 border-b border-border">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-xl bg-primary flex items-center justify-center">
            <Wifi className="text-white w-4 h-4" />
          </div>
          <span className="text-lg font-bold tracking-tight text-text-primary">Billing ISP</span>
        </div>
        {onClose && (
          <button onClick={onClose} className="lg:hidden p-1 text-text-muted hover:bg-slate-100 rounded-md">
            <X className="w-5 h-5" />
          </button>
        )}
      </div>
      <nav className="flex-1 space-y-1 p-4 overflow-y-auto">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.name}
              href={item.href}
              className="relative group flex items-center h-10 px-3"
            >
              <div
                className={cn(
                  "absolute inset-0 rounded-lg transition-colors duration-150",
                  isActive
                    ? "bg-primary/10"
                    : "group-hover:bg-slate-100"
                )}
              />
              {isActive && (
                <div className="absolute left-0 top-1.5 bottom-1.5 w-[3px] rounded-r-full bg-primary" />
              )}
              <div className="relative flex items-center gap-3 z-10 w-full pl-1">
                <Icon
                  className={cn(
                    "h-4 w-4 transition-colors",
                    isActive ? "text-primary" : "text-text-muted group-hover:text-text-primary"
                  )}
                />
                <span
                  className={cn(
                    "text-sm transition-colors",
                    isActive
                      ? "text-primary font-semibold"
                      : "text-text-muted font-medium group-hover:text-text-primary"
                  )}
                >
                  {item.name}
                </span>
              </div>
            </Link>
          );
        })}
      </nav>
    </div>
  );
}
