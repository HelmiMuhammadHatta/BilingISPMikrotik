"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { jwtDecode } from "jwt-decode";

interface AuthContextType {
  isAuthenticated: boolean;
  role: string | null;
  login: (token: string) => void;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [role, setRole] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        const userRole = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role;
        setRole(userRole);
        setIsAuthenticated(true);
      } catch (e) {
        localStorage.removeItem("token");
        setIsAuthenticated(false);
        setRole(null);
      }
    } else {
      setIsAuthenticated(false);
      setRole(null);
      if (pathname !== "/login") {
        router.push("/login");
      }
    }
    setLoading(false);
  }, [pathname, router]);

  const login = (token: string) => {
    localStorage.setItem("token", token);
    try {
      const decoded: any = jwtDecode(token);
      const userRole = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role;
      setRole(userRole);
      setIsAuthenticated(true);
      if (userRole === "Customer") {
        router.push("/customer-portal");
      } else {
        router.push("/");
      }
    } catch (e) {
      console.error("Invalid token");
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    setIsAuthenticated(false);
    setRole(null);
    router.push("/login");
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, role, login, logout, loading }}>
      {!loading && children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
