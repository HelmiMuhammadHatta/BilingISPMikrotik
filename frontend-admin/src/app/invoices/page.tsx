"use client";

import { useEffect, useState } from "react";
import { format, parseISO } from "date-fns";
import * as XLSX from "xlsx";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Download, Filter } from "lucide-react";

type Invoice = {
  id: string;
  customer: { name: string };
  servicePlan: { name: string };
  periodMonth: number;
  periodYear: number;
  amount: number;
  status: number; // 0=Unpaid, 1=Paid, 2=Overdue
  dueDate: string;
  paidAt: string | null;
};

export default function InvoicesPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<string>("all");

  useEffect(() => {
    async function fetchInvoices() {
      try {
        const res = await api.get("/invoices");
        setInvoices(res.data);
      } catch (error) {
        console.error("Failed to fetch invoices", error);
      } finally {
        setLoading(false);
      }
    }
    fetchInvoices();
  }, []);

  const filteredInvoices = invoices.filter((inv) => {
    if (statusFilter === "all") return true;
    return inv.status.toString() === statusFilter;
  });

  const handleExport = () => {
    const dataToExport = filteredInvoices.map((inv) => ({
      Customer: inv.customer?.name,
      Plan: inv.servicePlan?.name,
      Period: `${inv.periodMonth}/${inv.periodYear}`,
      Amount: inv.amount,
      Status: inv.status === 0 ? "Unpaid" : inv.status === 1 ? "Paid" : "Overdue",
      DueDate: format(parseISO(inv.dueDate), "yyyy-MM-dd"),
      PaidAt: inv.paidAt ? format(parseISO(inv.paidAt), "yyyy-MM-dd HH:mm") : "-",
    }));

    const worksheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, "Invoices");
    XLSX.writeFile(workbook, `Invoices_${format(new Date(), "yyyyMMdd")}.xlsx`);
  };

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 0:
        return <span className="rounded-full bg-orange-100 px-2.5 py-0.5 text-xs font-medium text-orange-800">Unpaid</span>;
      case 1:
        return <span className="rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">Paid</span>;
      case 2:
        return <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">Overdue</span>;
      default:
        return <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">Unknown</span>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900">Invoices</h1>
        <button
          onClick={handleExport}
          className="flex items-center gap-2 rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
        >
          <Download className="h-4 w-4" />
          Export Excel
        </button>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>All Invoices</CardTitle>
          <div className="flex items-center gap-2 text-sm">
            <Filter className="h-4 w-4 text-gray-500" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="rounded-md border-gray-300 p-1 text-gray-700 focus:border-blue-500 focus:ring-blue-500 border"
            >
              <option value="all">All Status</option>
              <option value="0">Unpaid</option>
              <option value="1">Paid</option>
              <option value="2">Overdue</option>
            </select>
          </div>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading data...</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-gray-500">
                <thead className="bg-gray-50 text-xs uppercase text-gray-700">
                  <tr>
                    <th className="px-6 py-3">Customer</th>
                    <th className="px-6 py-3">Period</th>
                    <th className="px-6 py-3">Amount</th>
                    <th className="px-6 py-3">Due Date</th>
                    <th className="px-6 py-3">Status</th>
                    <th className="px-6 py-3">Paid At</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredInvoices.map((inv) => (
                    <tr key={inv.id} className="border-b bg-white hover:bg-gray-50">
                      <td className="px-6 py-4 font-medium text-gray-900">{inv.customer?.name}</td>
                      <td className="px-6 py-4">{`${inv.periodMonth}/${inv.periodYear}`}</td>
                      <td className="px-6 py-4">Rp {inv.amount.toLocaleString("id-ID")}</td>
                      <td className="px-6 py-4">{format(parseISO(inv.dueDate), "dd MMM yyyy")}</td>
                      <td className="px-6 py-4">{getStatusBadge(inv.status)}</td>
                      <td className="px-6 py-4">
                        {inv.paidAt ? format(parseISO(inv.paidAt), "dd MMM yyyy HH:mm") : "-"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
