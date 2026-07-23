"use client";

import { AuthProvider, useAuth } from "@/context/AuthContext";
import { Sidebar } from "@/components/Sidebar";
import { Topbar } from "@/components/Topbar";
import { usePathname } from "next/navigation";

function LayoutContent({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { isAuthenticated } = useAuth();
  const isLoginPage = pathname === "/login";

  if (isLoginPage) {
    return <main className="flex-1 w-full">{children}</main>;
  }

  if (!isAuthenticated) {
    return null; // Will redirect in AuthProvider
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
