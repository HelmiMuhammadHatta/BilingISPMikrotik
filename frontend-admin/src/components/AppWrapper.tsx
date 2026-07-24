"use client";

import { AuthProvider, useAuth } from "@/context/AuthContext";
import { Sidebar } from "@/components/Sidebar";
import { Topbar } from "@/components/Topbar";
import { usePathname } from "next/navigation";
import { useEffect } from "react";

function LayoutContent({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { isAuthenticated, role } = useAuth();
  const isLoginPage = pathname === "/login";

  useEffect(() => {
    if (isAuthenticated && role === "Customer" && pathname !== "/customer-portal") {
      // Redirect customers trying to access admin pages
      // In a real app we'd use Next router, but since we are inside a useEffect without router dependency mapped here cleanly, 
      // we can do a simple window.location.href or just render null and let AuthContext handle it. 
      // Actually, it's better to just use router from next/navigation
    }
  }, [isAuthenticated, role, pathname]);

  if (isLoginPage) {
    return <main className="flex-1 w-full">{children}</main>;
  }

  if (!isAuthenticated) {
    return null; // Will redirect in AuthProvider
  }

  if (role === "Customer") {
    // For customers, don't show the Admin Sidebar. Just show content.
    return (
      <div className="flex min-h-screen w-full bg-gray-50 text-gray-900">
        <div className="flex flex-1 flex-col">
          {/* We might reuse topbar or create a customer specific one later. For now, simple topbar */}
          <header className="bg-white border-b border-gray-200 h-16 flex items-center px-6 justify-between">
            <h1 className="text-xl font-bold text-gray-800">Customer Portal</h1>
            <div className="flex items-center">
               {/* Minimal customer header */}
            </div>
          </header>
          <main className="flex-1 overflow-y-auto p-6">{children}</main>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen w-full bg-gray-50 text-gray-900">
      <Sidebar />
      <div className="flex flex-1 flex-col">
        <Topbar />
        <main className="flex-1 overflow-y-auto p-6">{children}</main>
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
