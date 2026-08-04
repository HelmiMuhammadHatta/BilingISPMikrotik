"use client";

import { AuthProvider, useAuth } from "@/context/AuthContext";
import { Sidebar } from "@/components/Sidebar";
import { Topbar } from "@/components/Topbar";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";

function LayoutContent({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { isAuthenticated, role } = useAuth();
  const isLoginPage = pathname === "/login";
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    // Close mobile menu on path change
    setIsMobileMenuOpen(false);
  }, [pathname]);

  if (isLoginPage) {
    return <main className="flex-1 w-full">{children}</main>;
  }

  if (!isAuthenticated) {
    return null; // Will redirect in AuthProvider
  }

  if (role === "Customer") {
    // For customers, don't show the Admin Sidebar. Just show content.
    return (
      <div className="flex min-h-screen w-full bg-surface-muted text-text-primary">
        <main className="flex-1 overflow-y-auto">{children}</main>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen w-full bg-surface-muted text-text-primary overflow-hidden relative">
      {/* Mobile Backdrop */}
      {isMobileMenuOpen && (
        <div 
          className="fixed inset-0 z-40 bg-slate-900/50 backdrop-blur-sm lg:hidden transition-opacity"
          onClick={() => setIsMobileMenuOpen(false)}
        />
      )}
      
      {/* Sidebar */}
      <div className={`fixed inset-y-0 left-0 z-50 w-[260px] transform transition-transform duration-200 ease-in-out lg:relative lg:translate-x-0 ${isMobileMenuOpen ? "translate-x-0" : "-translate-x-full"}`}>
        <Sidebar onClose={() => setIsMobileMenuOpen(false)} />
      </div>

      <div className="flex flex-1 flex-col overflow-hidden">
        <Topbar onMenuClick={() => setIsMobileMenuOpen(true)} />
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">{children}</main>
      </div>
    </div>
  );
}

export function AppWrapper({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <LayoutContent>{children}</LayoutContent>
    </AuthProvider>
  );
}
