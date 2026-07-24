"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { LogOut, User, Activity, CreditCard } from "lucide-react";
import { format } from "date-fns";

export default function CustomerPortal() {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [payingInvoiceId, setPayingInvoiceId] = useState<string | null>(null);
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
      await api.post(`/CustomerPortal/pay/${invoiceId}`);
      await fetchData();
    } catch (error) {
      console.error("Failed to pay invoice", error);
      alert("Payment failed. Please try again.");
    } finally {
      setPayingInvoiceId(null);
    }
  };

  if (loading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-gray-500">Loading your portal...</div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex flex-col h-full items-center justify-center">
        <div className="text-red-500 mb-4">Error loading data.</div>
        <button onClick={logout} className="px-4 py-2 bg-gray-200 rounded-lg">Logout</button>
      </div>
    );
  }

  const { customer, invoices } = data;

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex justify-between items-center bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <div>
          <h2 className="text-2xl font-bold text-gray-800">Welcome, {customer.name}</h2>
          <p className="text-gray-500">{customer.phone} | {customer.address}</p>
        </div>
        <button
          onClick={logout}
          className="flex items-center space-x-2 text-gray-500 hover:text-red-600 transition-colors bg-gray-50 hover:bg-red-50 px-4 py-2 rounded-lg border border-gray-200 hover:border-red-200"
        >
          <LogOut className="h-4 w-4" />
          <span>Logout</span>
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex items-start space-x-4">
          <div className="p-3 bg-blue-50 text-blue-600 rounded-lg">
            <User className="h-6 w-6" />
          </div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Service Plan</p>
            <h3 className="text-lg font-semibold text-gray-900 mt-1">{customer.servicePlan || "None"}</h3>
            <p className="text-xs text-gray-400 mt-1">{customer.speed}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex items-start space-x-4">
          <div className="p-3 bg-green-50 text-green-600 rounded-lg">
            <Activity className="h-6 w-6" />
          </div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Internet Status</p>
            <div className="mt-2">
              <span
                className={`px-3 py-1 text-xs font-semibold rounded-full ${
                  customer.status === 0
                    ? "bg-green-100 text-green-800"
                    : customer.status === 1
                    ? "bg-orange-100 text-orange-800"
                    : "bg-red-100 text-red-800"
                }`}
              >
                {customer.status === 0 ? "Active" : customer.status === 1 ? "Isolated" : "Suspended"}
              </span>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex items-start space-x-4">
          <div className="p-3 bg-purple-50 text-purple-600 rounded-lg">
            <CreditCard className="h-6 w-6" />
          </div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Monthly Fee</p>
            <h3 className="text-lg font-semibold text-gray-900 mt-1">
              Rp {customer.price?.toLocaleString("id-ID") || 0}
            </h3>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-5 border-b border-gray-100">
          <h3 className="text-lg font-semibold text-gray-900">Your Invoices</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50/50">
                <th className="py-4 px-6 text-xs font-semibold text-gray-500 uppercase tracking-wider">Period</th>
                <th className="py-4 px-6 text-xs font-semibold text-gray-500 uppercase tracking-wider">Due Date</th>
                <th className="py-4 px-6 text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
                <th className="py-4 px-6 text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
                <th className="py-4 px-6 text-xs font-semibold text-gray-500 uppercase tracking-wider text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {invoices.length === 0 ? (
                <tr>
                  <td colSpan={5} className="py-8 text-center text-gray-500">
                    No invoices found.
                  </td>
                </tr>
              ) : (
                invoices.map((inv: any) => (
                  <tr key={inv.id} className="hover:bg-gray-50 transition-colors">
                    <td className="py-4 px-6 text-sm text-gray-900 font-medium">
                      {format(new Date(inv.periodYear, inv.periodMonth - 1), "MMM yyyy")}
                    </td>
                    <td className="py-4 px-6 text-sm text-gray-500">
                      {format(new Date(inv.dueDate), "dd MMM yyyy")}
                    </td>
                    <td className="py-4 px-6 text-sm text-gray-900 font-medium">
                      Rp {inv.amount.toLocaleString("id-ID")}
                    </td>
                    <td className="py-4 px-6">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          inv.status === 1
                            ? "bg-green-100 text-green-800"
                            : inv.status === 2
                            ? "bg-red-100 text-red-800"
                            : "bg-yellow-100 text-yellow-800"
                        }`}
                      >
                        {inv.status === 1 ? "Paid" : inv.status === 2 ? "Overdue" : "Unpaid"}
                      </span>
                    </td>
                    <td className="py-4 px-6 text-right">
                      {inv.status !== 1 && (
                        <button
                          onClick={() => handlePay(inv.id)}
                          disabled={payingInvoiceId === inv.id}
                          className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 transition-colors"
                        >
                          {payingInvoiceId === inv.id ? "Processing..." : "Pay Now"}
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
