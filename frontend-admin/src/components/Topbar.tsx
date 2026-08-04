"use client";

import { useState, useEffect, useRef } from "react";
import { Bell, Search, LogOut, Menu } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";
import { format, parseISO } from "date-fns";

type Notification = {
  type: string;
  title: string;
  message: string;
  timestamp: string;
};

export function Topbar({ onMenuClick }: { onMenuClick?: () => void }) {
  const { logout } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [showNotifications, setShowNotifications] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    async function fetchNotifications() {
      try {
        const res = await api.get("/dashboard/notifications");
        setNotifications(res.data);
      } catch (error) {
        console.error("Failed to fetch notifications", error);
      }
    }
    fetchNotifications();
  }, []);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowNotifications(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <header className="flex h-16 shrink-0 items-center justify-between border-b border-border bg-surface px-4 lg:px-6">
      <div className="flex items-center gap-4">
        {onMenuClick && (
          <button 
            onClick={onMenuClick}
            className="lg:hidden p-2 -ml-2 text-text-muted hover:bg-slate-100 rounded-md"
          >
            <Menu className="h-5 w-5" />
          </button>
        )}
        <div className="hidden sm:flex items-center gap-2 text-text-muted">
          <Search className="h-4 w-4" />
          <span className="text-sm">Search...</span>
        </div>
      </div>
      
      <div className="flex items-center gap-4 relative">
        <div className="relative" ref={dropdownRef}>
          <button 
            className="relative rounded-full p-2 hover:bg-slate-100 transition-colors text-text-muted"
            onClick={() => setShowNotifications(!showNotifications)}
          >
            <Bell className="h-5 w-5" />
            {notifications.length > 0 && (
              <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-danger ring-2 ring-surface"></span>
            )}
          </button>
          
          {showNotifications && (
            <div className="absolute right-0 mt-2 w-80 rounded-xl shadow-lg bg-surface border border-border z-50">
              <div className="p-3 border-b border-border">
                <h3 className="text-sm font-semibold text-text-primary">Notifications</h3>
              </div>
              <div className="max-h-96 overflow-y-auto">
                {notifications.length === 0 ? (
                  <div className="p-4 text-sm text-center text-text-muted">No new notifications</div>
                ) : (
                  notifications.map((notif, idx) => (
                    <div key={idx} className="p-3 hover:bg-slate-50 border-b border-border last:border-0 transition-colors cursor-pointer">
                      <div className="flex justify-between items-start">
                        <p className="text-sm font-medium text-text-primary">{notif.title}</p>
                        <span className="text-xs text-text-muted">
                          {format(parseISO(notif.timestamp), "HH:mm")}
                        </span>
                      </div>
                      <p className="text-sm text-text-muted mt-1">{notif.message}</p>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

        <div className="flex items-center gap-2 h-9 rounded-full bg-primary pl-4 pr-1.5 text-white font-medium text-sm shadow-sm">
          <span>Admin</span>
          <button 
            onClick={logout}
            className="p-1.5 rounded-full bg-white/10 hover:bg-white/20 transition-colors ml-1"
            title="Logout"
          >
            <LogOut className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>
    </header>
  );
}
