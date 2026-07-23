"use client";

import { Bell, Search, LogOut } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

export function Topbar() {
  const { logout } = useAuth();

  return (
    <header className="flex h-16 items-center justify-between border-b bg-white px-6">
      <div className="flex items-center gap-4 text-gray-500">
        <Search className="h-5 w-5" />
        <span className="text-sm">Search...</span>
      </div>
      <div className="flex items-center gap-4">
        <button className="relative rounded-full p-2 hover:bg-gray-100">
          <Bell className="h-5 w-5 text-gray-500" />
          <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-red-500"></span>
        </button>
        <div className="flex items-center gap-2 h-8 rounded-full bg-blue-500 pl-3 pr-1 text-white font-medium text-sm">
          <span>Admin</span>
          <button 
            onClick={logout}
            className="p-1 rounded-full hover:bg-blue-600 transition-colors ml-1"
            title="Logout"
          >
            <LogOut className="h-4 w-4" />
          </button>
        </div>
      </div>
    </header>
  );
}
