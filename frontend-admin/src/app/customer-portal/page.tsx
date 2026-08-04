"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { LogOut, User, Activity, CreditCard, ReceiptText, Calendar, Clock, ChevronRight, X, AlertCircle } from "lucide-react";
import { format } from "date-fns";
import Script from "next/script";

type Invoice = {
  id: string;
  periodYear: number;
  periodMonth: number;
  dueDate: string;
  amount: number;
  status: number;
  [key: string]: any;
};

export default function CustomerPortal() {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [payingInvoiceId, setPayingInvoiceId] = useState<string | null>(null);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const { logout, role } = useAuth();

  useEffect(() => {
    if (role === "Customer") {
      fetchData();
    }
  }, [role]);

  const fetchData = async () => {
    try {
      const response = await api.get("/CustomerPortal/my-data");
      setData(response.data);
    } catch (error) {
      console.error("Failed to fetch customer data", error);
    } finally {
      setLoading(false);
    }
  };

  const handlePay = async (invoiceId: string) => {
    setPayingInvoiceId(invoiceId);
    try {
      const response = await api.post(`/CustomerPortal/invoices/${invoiceId}/create-payment`);
      const snapToken = response.data.token;

      if (typeof window !== "undefined" && (window as any).snap) {
        (window as any).snap.pay(snapToken, {
          onSuccess: function (result: any) {
            console.log("Payment success!", result);
            alert("Payment success!");
            fetchData();
            setSelectedInvoice(null);
          },
          onPending: function (result: any) {
            console.log("Payment pending", result);
            alert("Waiting your payment!");
            fetchData();
            setSelectedInvoice(null);
          },
          onError: function (result: any) {
            console.log("Payment error", result);
            alert("Payment failed!");
          },
          onClose: function () {
            console.log("customer closed the popup without finishing the payment");
          }
        });
      } else {
        alert("Payment gateway is not loaded yet.");
      }
    } catch (error) {
      console.error("Failed to initiate payment", error);
      alert("Failed to initiate payment. Please try again.");
    } finally {
      setPayingInvoiceId(null);
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <div className="flex flex-col items-center gap-3">
          <div className="w-8 h-8 border-4 border-primary/30 border-t-primary rounded-full animate-spin" />
          <div className="text-text-muted font-medium">Memuat portal...</div>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <div className="p-4 bg-danger/10 rounded-full text-danger">
          <AlertCircle className="w-8 h-8" />
        </div>
        <div className="text-text-primary font-medium">Gagal memuat data.</div>
        <button 
          onClick={logout} 
          className="px-6 py-2 bg-surface border border-border rounded-lg text-text-primary hover:bg-slate-50 transition-colors"
        >
          Logout
        </button>
      </div>
    );
  }

  const { customer, invoices } = data;

  const getStatusBadge = (status: number) => {
    if (status === 0) return { bg: "bg-success/10", text: "text-success", label: "Aktif", dot: "bg-success animate-pulse" };
    if (status === 1) return { bg: "bg-warning/10", text: "text-warning", label: "Terisolir", dot: "bg-warning" };
    return { bg: "bg-danger/10", text: "text-danger", label: "Suspend", dot: "bg-danger" };
  };
  const customerStatus = getStatusBadge(customer.status);

  return (
    <div className="max-w-6xl mx-auto space-y-6 pb-20 lg:pb-8">
      <Script
        src="https://app.sandbox.midtrans.com/snap/snap.js"
        data-client-key={process.env.NEXT_PUBLIC_MIDTRANS_CLIENT_KEY}
      />
      
      {/* Welcome Banner */}
      <div className="relative overflow-hidden rounded-2xl bg-primary text-white p-6 lg:p-8 shadow-md">
        <div className="absolute inset-0 opacity-10 bg-[radial-gradient(#ffffff_1px,transparent_1px)] [background-size:24px_24px]"></div>
        <div className="absolute -right-20 -top-20 w-64 h-64 bg-white/10 rounded-full blur-3xl"></div>
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="flex items-center gap-4">
            <div className="w-14 h-14 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center text-xl font-bold border border-white/20">
              {customer.name.charAt(0).toUpperCase()}
            </div>
            <div>
              <h2 className="text-2xl font-bold mb-1">Halo, {customer.name}</h2>
              <p className="text-blue-100 text-sm">{customer.phone} • {customer.address}</p>
            </div>
          </div>
          <button
            onClick={logout}
            className="self-start md:self-auto flex items-center gap-2 bg-white/10 hover:bg-white/20 text-white px-4 py-2 rounded-lg transition-colors border border-white/10 text-sm font-medium backdrop-blur-sm"
          >
            <LogOut className="h-4 w-4" />
            <span>Logout</span>
          </button>
        </div>
      </div>

      {/* Info Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Service Plan */}
        <div className="bg-surface border border-border border-l-4 border-l-primary rounded-xl p-5 shadow-sm hover:shadow-md transition-all">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-text-muted mb-1">Paket Internet</p>
              <h3 className="text-lg font-bold text-text-primary">{customer.servicePlan || "Tidak Ada"}</h3>
              <p className="text-sm text-text-muted mt-1 font-medium">{customer.speed}</p>
            </div>
            <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
              <Activity className="w-5 h-5 text-primary" />
            </div>
          </div>
        </div>

        {/* Internet Status */}
        <div className={`bg-surface border border-border border-l-4 rounded-xl p-5 shadow-sm hover:shadow-md transition-all ${customerStatus.text.replace('text-', 'border-l-')}`}>
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-text-muted mb-2">Status Koneksi</p>
              <div className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-semibold ${customerStatus.bg} ${customerStatus.text}`}>
                <span className={`w-2 h-2 rounded-full ${customerStatus.dot}`}></span>
                {customerStatus.label}
              </div>
            </div>
            <div className={`w-10 h-10 rounded-lg ${customerStatus.bg} flex items-center justify-center`}>
              <User className={`w-5 h-5 ${customerStatus.text}`} />
            </div>
          </div>
        </div>

        {/* Monthly Fee */}
        <div className="bg-surface border border-border border-l-4 border-l-warning rounded-xl p-5 shadow-sm hover:shadow-md transition-all">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-text-muted mb-1">Tagihan Bulanan</p>
              <h3 className="text-xl font-bold text-text-primary tabular-nums tracking-tight">
                Rp {(customer.price || 0).toLocaleString("id-ID")}
              </h3>
            </div>
            <div className="w-10 h-10 rounded-lg bg-warning/10 flex items-center justify-center">
              <CreditCard className="w-5 h-5 text-warning" />
            </div>
          </div>
        </div>
      </div>

      {/* Invoice List */}
      <div className="bg-surface border border-border rounded-xl shadow-sm overflow-hidden">
        <div className="px-6 py-5 border-b border-border flex items-center gap-2">
          <ReceiptText className="w-5 h-5 text-text-muted" />
          <h3 className="text-lg font-semibold text-text-primary">Daftar Tagihan Anda</h3>
        </div>
        
        {invoices.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
            <div className="w-16 h-16 bg-surface-muted rounded-full flex items-center justify-center mb-4">
              <CheckCircle2 className="w-8 h-8 text-success opacity-50" />
            </div>
            <h4 className="text-text-primary font-semibold mb-1">Semua Lunas!</h4>
            <p className="text-text-muted text-sm">Tidak ada tagihan yang belum dibayar saat ini.</p>
          </div>
        ) : (
          <>
            {/* Desktop Table View */}
            <div className="hidden lg:block overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-surface-muted border-b border-border">
                    <th className="py-4 px-6 text-xs font-semibold text-text-muted uppercase tracking-wider">Periode</th>
                    <th className="py-4 px-6 text-xs font-semibold text-text-muted uppercase tracking-wider">Jatuh Tempo</th>
                    <th className="py-4 px-6 text-xs font-semibold text-text-muted uppercase tracking-wider">Nominal</th>
                    <th className="py-4 px-6 text-xs font-semibold text-text-muted uppercase tracking-wider">Status</th>
                    <th className="py-4 px-6 text-xs font-semibold text-text-muted uppercase tracking-wider text-right">Aksi</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {invoices.map((inv: Invoice) => (
                    <tr 
                      key={inv.id} 
                      className="hover:bg-slate-50 transition-colors cursor-pointer group"
                      onClick={() => setSelectedInvoice(inv)}
                    >
                      <td className="py-4 px-6 text-sm text-text-primary font-medium">
                        {format(new Date(inv.periodYear, inv.periodMonth - 1), "MMMM yyyy")}
                      </td>
                      <td className="py-4 px-6 text-sm text-text-muted">
                        {format(new Date(inv.dueDate), "dd MMM yyyy")}
                      </td>
                      <td className="py-4 px-6 text-sm text-text-primary font-bold tabular-nums">
                        Rp {inv.amount.toLocaleString("id-ID")}
                      </td>
                      <td className="py-4 px-6">
                        <span className={`inline-flex items-center px-2.5 py-1 rounded-md text-xs font-semibold ${
                            inv.status === 1 ? "bg-success/10 text-success" : 
                            inv.status === 2 ? "bg-danger/10 text-danger" : 
                            "bg-warning/10 text-warning"
                          }`}
                        >
                          {inv.status === 1 ? "Lunas" : inv.status === 2 ? "Terlambat" : "Belum Bayar"}
                        </span>
                      </td>
                      <td className="py-4 px-6 text-right">
                        <ChevronRight className="w-5 h-5 text-text-muted inline-block group-hover:text-primary transition-colors" />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Mobile Card View */}
            <div className="lg:hidden divide-y divide-border">
              {invoices.map((inv: Invoice) => (
                <div 
                  key={inv.id} 
                  className="p-4 active:bg-slate-50 transition-colors"
                  onClick={() => setSelectedInvoice(inv)}
                >
                  <div className="flex justify-between items-start mb-2">
                    <div>
                      <h4 className="text-text-primary font-semibold text-sm">
                        Tagihan {format(new Date(inv.periodYear, inv.periodMonth - 1), "MMMM yyyy")}
                      </h4>
                      <div className="flex items-center gap-1.5 text-xs text-text-muted mt-1">
                        <Clock className="w-3.5 h-3.5" />
                        Jatuh tempo: {format(new Date(inv.dueDate), "dd MMM yyyy")}
                      </div>
                    </div>
                    <span className={`inline-flex items-center px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider ${
                        inv.status === 1 ? "bg-success/10 text-success" : 
                        inv.status === 2 ? "bg-danger/10 text-danger" : 
                        "bg-warning/10 text-warning"
                      }`}
                    >
                      {inv.status === 1 ? "Lunas" : inv.status === 2 ? "Terlambat" : "Belum Bayar"}
                    </span>
                  </div>
                  <div className="flex justify-between items-center mt-3">
                    <span className="text-lg font-bold text-text-primary tabular-nums tracking-tight">
                      Rp {inv.amount.toLocaleString("id-ID")}
                    </span>
                    <button className="text-sm font-medium text-primary flex items-center gap-1">
                      Detail <ChevronRight className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Invoice Detail Modal (Dialog / Bottom Sheet) */}
      {selectedInvoice && (
        <>
          <div 
            className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-40 transition-opacity"
            onClick={() => setSelectedInvoice(null)}
          />
          <div className="fixed inset-x-0 bottom-0 lg:inset-0 z-50 flex items-end lg:items-center justify-center p-0 lg:p-4 pointer-events-none">
            <div className="bg-surface w-full lg:max-w-md rounded-t-2xl lg:rounded-2xl shadow-2xl overflow-hidden pointer-events-auto transform transition-transform duration-300 ease-out translate-y-0 max-h-[90vh] flex flex-col">
              
              {/* Header */}
              <div className="px-6 py-4 border-b border-border flex items-center justify-between sticky top-0 bg-surface z-10">
                <h3 className="text-lg font-bold text-text-primary">Detail Tagihan</h3>
                <button 
                  onClick={() => setSelectedInvoice(null)}
                  className="p-1.5 text-text-muted hover:bg-slate-100 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              {/* Content */}
              <div className="p-6 overflow-y-auto space-y-6">
                <div className="text-center">
                  <p className="text-sm text-text-muted font-medium mb-1">Nominal Tagihan</p>
                  <p className="text-3xl font-bold text-text-primary tabular-nums tracking-tight">
                    Rp {selectedInvoice.amount.toLocaleString("id-ID")}
                  </p>
                  <div className="mt-3 flex justify-center">
                    <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest ${
                        selectedInvoice.status === 1 ? "bg-success/10 text-success" : 
                        selectedInvoice.status === 2 ? "bg-danger/10 text-danger" : 
                        "bg-warning/10 text-warning"
                      }`}
                    >
                      {selectedInvoice.status === 1 ? "Lunas" : selectedInvoice.status === 2 ? "Terlambat" : "Belum Dibayar"}
                    </span>
                  </div>
                </div>

                <div className="border border-border rounded-xl divide-y divide-border">
                  <div className="p-4 flex justify-between items-center">
                    <span className="text-sm text-text-muted">Periode Tagihan</span>
                    <span className="text-sm font-semibold text-text-primary">
                      {format(new Date(selectedInvoice.periodYear, selectedInvoice.periodMonth - 1), "MMMM yyyy")}
                    </span>
                  </div>
                  <div className="p-4 flex justify-between items-center">
                    <span className="text-sm text-text-muted">Jatuh Tempo</span>
                    <span className="text-sm font-semibold text-text-primary">
                      {format(new Date(selectedInvoice.dueDate), "dd MMMM yyyy")}
                    </span>
                  </div>
                  <div className="p-4 flex justify-between items-center">
                    <span className="text-sm text-text-muted">ID Invoice</span>
                    <span className="text-xs font-mono bg-surface-muted px-2 py-1 rounded text-text-primary">
                      {selectedInvoice.id.substring(0, 8).toUpperCase()}...
                    </span>
                  </div>
                </div>

                {selectedInvoice.status !== 1 && (
                  <div className="pt-2">
                    <button
                      onClick={() => handlePay(selectedInvoice.id)}
                      disabled={payingInvoiceId === selectedInvoice.id}
                      className="w-full h-12 flex justify-center items-center rounded-xl font-bold text-white bg-primary hover:bg-primary-hover active:scale-[0.98] focus:outline-none focus:ring-4 focus:ring-primary/20 disabled:opacity-70 disabled:cursor-not-allowed transition-all duration-200 shadow-md shadow-primary/20"
                    >
                      {payingInvoiceId === selectedInvoice.id ? "Memproses..." : "Bayar Sekarang (Midtrans)"}
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function CheckCircle2(props: any) {
  return (
    <svg
      {...props}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <circle cx="12" cy="12" r="10" />
      <path d="m9 12 2 2 4-4" />
    </svg>
  );
}
