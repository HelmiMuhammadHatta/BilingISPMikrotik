"use client";

import { useState, useEffect, useRef } from "react";
import { Bell, Search, LogOut } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";
import { format, parseISO } from "date-fns";

type Notification = {
  type: string;
  title: string;
  message: string;
  timestamp: string;
};

export function Topbar() {
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
    <header className="flex h-16 items-center justify-between border-b bg-white px-6">
      <div className="flex items-center gap-4 text-gray-500">
        <Search className="h-5 w-5" />
        <span className="text-sm">Search...</span>
      </div>
      <div className="flex items-center gap-4 relative">
        <div className="relative" ref={dropdownRef}>
          <button 
            className="relative rounded-full p-2 hover:bg-gray-100"
            onClick={() => setShowNotifications(!showNotifications)}
          >
            <Bell className="h-5 w-5 text-gray-500" />
            {notifications.length > 0 && (
              <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-red-500"></span>
            )}
          </button>
          
          {showNotifications && (
            <div className="absolute right-0 mt-2 w-80 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5 z-50">
              <div className="p-3 border-b border-gray-100">
                <h3 className="text-sm font-semibold text-gray-900">Notifications</h3>
              </div>
              <div className="max-h-96 overflow-y-auto">
                {notifications.length === 0 ? (
                  <div className="p-4 text-sm text-center text-gray-500">No new notifications</div>
                ) : (
                  notifications.map((notif, idx) => (
                    <div key={idx} className="p-3 hover:bg-gray-50 border-b border-gray-100 last:border-0">
                      <div className="flex justify-between items-start">
                        <p className="text-sm font-medium text-gray-900">{notif.title}</p>
                        <span className="text-xs text-gray-500">
                          {format(parseISO(notif.timestamp), "HH:mm")}
                        </span>
                      </div>
                      <p className="text-sm text-gray-500 mt-1">{notif.message}</p>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

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
