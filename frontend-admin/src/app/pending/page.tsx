"use client";

import { useEffect, useState } from "react";
import { format, isPast, isToday, parseISO } from "date-fns";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { AlertCircle } from "lucide-react";

type Invoice = {
  id: string;
  customerId: string;
  customer: { name: string };
  servicePlanId: string;
  servicePlan: { name: string; price: number };
  periodMonth: number;
  periodYear: number;
  amount: number;
  status: number; // 0 = Unpaid, 1 = Paid, 2 = Overdue
  dueDate: string;
};

export default function PendingInvoicesPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchPending() {
      try {
        const res = await api.get("/invoices"); // We need to filter this client side or backend
        // We'll filter client side for now to get Unpaid or Overdue that is due today or past
        const data: Invoice[] = res.data;
        const filtered = data.filter((inv) => {
          if (inv.status === 1) return false; // Paid
          const dueDate = parseISO(inv.dueDate);
          return isToday(dueDate) || isPast(dueDate);
        });
        setInvoices(filtered);
      } catch (error) {
        console.error("Failed to fetch invoices", error);
      } finally {
        setLoading(false);
      }
    }
    fetchPending();
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900">Pending & Expired Invoices</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertCircle className="h-5 w-5 text-red-500" />
            Requires Attention ({invoices.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading data...</div>
          ) : invoices.length === 0 ? (
            <div className="py-8 text-center text-gray-500">No pending or expired invoices.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-gray-500">
                <thead className="bg-gray-50 text-xs uppercase text-gray-700">
                  <tr>
                    <th className="px-6 py-3">Customer</th>
                    <th className="px-6 py-3">Service Plan</th>
                    <th className="px-6 py-3">Amount</th>
                    <th className="px-6 py-3">Due Date</th>
                    <th className="px-6 py-3">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {invoices.map((inv) => {
                    const dueDate = parseISO(inv.dueDate);
                    const expired = isPast(dueDate) && !isToday(dueDate);
                    return (
                      <tr key={inv.id} className="border-b bg-white hover:bg-gray-50">
                        <td className="px-6 py-4 font-medium text-gray-900">
                          {inv.customer?.name || "Unknown"}
                        </td>
                        <td className="px-6 py-4">{inv.servicePlan?.name || "N/A"}</td>
                        <td className="px-6 py-4">Rp {inv.amount.toLocaleString("id-ID")}</td>
                        <td className="px-6 py-4">{format(dueDate, "dd MMM yyyy")}</td>
                        <td className="px-6 py-4">
                          <span
                            className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                              expired
                                ? "bg-red-100 text-red-800"
                                : "bg-orange-100 text-orange-800"
                            }`}
                          >
                            {expired ? "Expired / Overdue" : "Due Today"}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
