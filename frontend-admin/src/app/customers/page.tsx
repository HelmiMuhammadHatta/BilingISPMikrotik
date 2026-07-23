"use client";

import { useEffect, useState } from "react";
import { format, parseISO } from "date-fns";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Plus, Edit2 } from "lucide-react";

type Customer = {
  id: string;
  name: string;
  address: string;
  phone: string;
  pppUsername: string;
  status: number; // 0=Active, 1=Isolir, 2=Suspended
  servicePlanId: string;
  servicePlan?: { name: string; price: number };
  createdAt: string;
};

export default function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchCustomers() {
      try {
        const res = await api.get("/customers");
        setCustomers(res.data);
      } catch (error) {
        console.error("Failed to fetch customers", error);
      } finally {
        setLoading(false);
      }
    }
    fetchCustomers();
  }, []);

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 0:
        return <span className="rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">Active</span>;
      case 1:
        return <span className="rounded-full bg-orange-100 px-2.5 py-0.5 text-xs font-medium text-orange-800">Isolir</span>;
      case 2:
        return <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">Suspended</span>;
      default:
        return <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">Unknown</span>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900">Customers</h1>
        <button className="flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700">
          <Plus className="h-4 w-4" />
          Add Customer
        </button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Customers</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading data...</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-gray-500">
                <thead className="bg-gray-50 text-xs uppercase text-gray-700">
                  <tr>
                    <th className="px-6 py-3">Name</th>
                    <th className="px-6 py-3">PPP Username</th>
                    <th className="px-6 py-3">Service Plan</th>
                    <th className="px-6 py-3">Phone</th>
                    <th className="px-6 py-3">Status</th>
                    <th className="px-6 py-3">Created</th>
                    <th className="px-6 py-3 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {customers.map((cust) => (
                    <tr key={cust.id} className="border-b bg-white hover:bg-gray-50">
                      <td className="px-6 py-4 font-medium text-gray-900">{cust.name}</td>
                      <td className="px-6 py-4">{cust.pppUsername}</td>
                      <td className="px-6 py-4">{cust.servicePlan?.name || "No Plan"}</td>
                      <td className="px-6 py-4">{cust.phone}</td>
                      <td className="px-6 py-4">{getStatusBadge(cust.status)}</td>
                      <td className="px-6 py-4">{format(parseISO(cust.createdAt), "dd MMM yyyy")}</td>
                      <td className="px-6 py-4 text-right">
                        <button className="text-blue-600 hover:text-blue-900">
                          <Edit2 className="h-4 w-4" />
                        </button>
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
