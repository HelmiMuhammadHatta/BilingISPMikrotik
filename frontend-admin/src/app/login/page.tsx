"use client";

import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";
import { Lock, User, Wifi, CheckCircle2, Eye, EyeOff, Loader2 } from "lucide-react";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      const response = await api.post("/Auth/login", {
        username,
        password,
      });
      
      if (response.data.token) {
        login(response.data.token);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "Login failed. Please check your credentials.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen w-full bg-surface-muted">
      {/* Kolom Kiri - Hidden on mobile, 60% width on desktop */}
      <div className="hidden lg:flex lg:w-[60%] relative flex-col justify-between p-10 overflow-hidden" 
           style={{ background: "linear-gradient(135deg, #0B1220 0%, #16305C 100%)" }}>
        
        {/* Decorative elements */}
        <div className="absolute inset-0 opacity-10 bg-[radial-gradient(#ffffff_1px,transparent_1px)] [background-size:24px_24px]"></div>
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-primary/20 rounded-full blur-[100px]"></div>

        <div className="relative z-10">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-2xl bg-primary flex items-center justify-center shadow-lg">
              <Wifi className="text-white w-6 h-6" />
            </div>
            <span className="text-white font-bold text-2xl tracking-tight">Billing ISP</span>
          </div>
        </div>

        <div className="relative z-10 max-w-xl mt-auto mb-20">
          <h1 className="text-4xl md:text-5xl font-bold text-white leading-tight mb-8">
            Kelola tagihan pelanggan dengan mudah.
          </h1>
          <div className="space-y-4">
            {[
              "Monitoring tagihan secara realtime",
              "Pembuatan invoice pelanggan otomatis",
              "Isolir pelanggan yang menunggak secara otomatis"
            ].map((feature, idx) => (
              <div key={idx} className="flex items-center gap-3 text-slate-300">
                <CheckCircle2 className="w-5 h-5 text-success flex-shrink-0" />
                <span className="text-lg">{feature}</span>
              </div>
            ))}
          </div>
        </div>
        
        <div className="relative z-10 text-slate-500 text-sm">
          Sistem Penagihan Terpadu v2.0
        </div>
      </div>

      {/* Kolom Kanan - Full width on mobile, 40% on desktop */}
      <div className="flex flex-col justify-center items-center w-full lg:w-[40%] p-6 lg:p-10 bg-surface">
        <div className="w-full max-w-sm">
          
          <div className="mb-8 text-center lg:text-left">
            <h2 className="text-2xl font-semibold text-text-primary mb-2">Masuk ke Dashboard</h2>
            <p className="text-text-muted text-sm">Silakan masukkan kredensial Anda untuk melanjutkan.</p>
          </div>

          {error && (
            <div className="bg-danger/10 border border-danger/20 text-danger text-sm p-3 rounded-lg mb-6 flex items-center gap-2">
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-[20px]">
            <div>
              <label className="block text-sm font-medium text-text-primary mb-1.5">
                Username
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                  <User className="h-4 w-4 text-text-muted" />
                </div>
                <input
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="block w-full h-11 pl-10 pr-4 bg-surface border border-border rounded-lg text-text-primary placeholder:text-text-muted/60 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all duration-200"
                  placeholder="Enter your username"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-text-primary mb-1.5">
                Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                  <Lock className="h-4 w-4 text-text-muted" />
                </div>
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full h-11 pl-10 pr-10 bg-surface border border-border rounded-lg text-text-primary placeholder:text-text-muted/60 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all duration-200"
                  placeholder="Enter your password"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3.5 flex items-center text-text-muted hover:text-text-primary transition-colors focus:outline-none"
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full h-11 flex justify-center items-center rounded-lg shadow-sm font-medium text-white bg-primary hover:bg-primary-hover active:scale-[0.99] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary/50 disabled:opacity-70 disabled:cursor-not-allowed transition-all duration-200 mt-2"
            >
              {isLoading ? (
                <>
                  <Loader2 className="h-5 w-5 animate-spin mr-2" />
                  Memproses...
                </>
              ) : (
                "Sign In"
              )}
            </button>
          </form>

          <div className="mt-12 text-center text-xs text-text-muted">
            &copy; 2026 Billing ISP
          </div>
        </div>
      </div>
    </div>
  );
}
