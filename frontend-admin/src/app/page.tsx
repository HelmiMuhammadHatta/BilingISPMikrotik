"use client";

import { useEffect, useState } from "react";
import { Users, AlertTriangle, Wallet, Clock, BarChart } from "lucide-react";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { api } from "@/lib/api";

type Stats = {
  totalActiveCustomers: number;
  totalIsolirCustomers: number;
  revenueThisMonth: number;
  pendingInvoicesCount: number;
  revenueChartData: { name: string; revenue: number }[];
};

export default function OverviewPage() {
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchStats() {
      try {
        const res = await api.get("/dashboard/stats");
        setStats(res.data);
      } catch (error) {
        console.error("Failed to fetch stats", error);
      } finally {
        setLoading(false);
      }
    }
    fetchStats();
  }, []);

  if (loading) {
    return <div className="flex h-full items-center justify-center text-text-muted animate-pulse">Loading dashboard...</div>;
  }

  if (!stats) return null;

  const statCards = [
    {
      title: "Active Customers",
      value: stats.totalActiveCustomers,
      icon: Users,
      colorClass: "success",
      borderColor: "border-l-success",
      bgBubble: "bg-success/10",
      iconColor: "text-success",
    },
    {
      title: "Isolated Customers",
      value: stats.totalIsolirCustomers,
      icon: AlertTriangle,
      colorClass: "danger",
      borderColor: "border-l-danger",
      bgBubble: "bg-danger/10",
      iconColor: "text-danger",
    },
    {
      title: "Revenue (This Month)",
      value: `Rp ${stats.revenueThisMonth.toLocaleString("id-ID")}`,
      icon: Wallet,
      colorClass: "primary",
      borderColor: "border-l-primary",
      bgBubble: "bg-primary/10",
      iconColor: "text-primary",
    },
    {
      title: "Pending Invoices",
      value: stats.pendingInvoicesCount,
      icon: Clock,
      colorClass: "warning",
      borderColor: "border-l-warning",
      bgBubble: "bg-warning/10",
      iconColor: "text-warning",
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight text-text-primary">Dashboard Overview</h1>

      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 xl:grid-cols-4">
        {statCards.map((card, idx) => (
          <div 
            key={idx}
            className={`bg-surface border border-border border-l-4 ${card.borderColor} rounded-xl p-5 shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all duration-200`}
          >
            <div className="flex justify-between items-start">
              <div className="space-y-2">
                <p className="text-sm font-medium text-text-muted">{card.title}</p>
                <p className="text-3xl font-semibold text-text-primary tabular-nums tracking-tight">
                  {card.value}
                </p>
              </div>
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${card.bgBubble}`}>
                <card.icon className={`w-5 h-5 ${card.iconColor}`} />
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-surface border border-border rounded-xl shadow-sm p-6 overflow-hidden">
        <h2 className="text-lg font-semibold text-text-primary mb-6">Revenue Overview</h2>
        <div className="h-[320px] w-full">
          {!stats.revenueChartData || stats.revenueChartData.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-full text-text-muted">
              <div className="w-16 h-16 rounded-2xl bg-surface-muted flex items-center justify-center mb-4">
                <BarChart className="w-8 h-8 opacity-50" />
              </div>
              <p className="text-sm font-medium">Belum ada data pendapatan</p>
            </div>
          ) : (
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={stats.revenueChartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <defs>
                  <linearGradient id="revenueFill" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#2563EB" stopOpacity={0.35}/>
                    <stop offset="100%" stopColor="#2563EB" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                <XAxis 
                  dataKey="name" 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{ fill: '#64748B', fontSize: 12 }} 
                  dy={10}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: '#64748B', fontSize: 12 }}
                  tickFormatter={(value) => `Rp ${(value / 1000000).toLocaleString("id-ID", { minimumFractionDigits: 0, maximumFractionDigits: 1 })}jt`}
                />
                <Tooltip 
                  cursor={{ stroke: '#94a3b8', strokeWidth: 1, strokeDasharray: '4 4' }}
                  content={({ active, payload, label }) => {
                    if (active && payload && payload.length) {
                      return (
                        <div className="bg-surface rounded-lg shadow-md border border-border p-3">
                          <p className="text-sm font-semibold text-text-primary mb-1">{label}</p>
                          <p className="text-sm text-primary font-medium">
                            Rp {payload[0].value?.toLocaleString("id-ID")}
                          </p>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
                <Area 
                  type="monotone" 
                  dataKey="revenue" 
                  stroke="#2563EB" 
                  strokeWidth={2} 
                  fill="url(#revenueFill)" 
                  dot={false} 
                  activeDot={{ r: 4, strokeWidth: 0, fill: '#2563EB' }} 
                />
              </AreaChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>
    </div>
  );
}
