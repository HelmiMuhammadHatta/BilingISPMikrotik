"use client";

import { useEffect, useState } from "react";
import { Plus, Edit2, Trash2 } from "lucide-react";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ServicePlanModal } from "@/components/ServicePlanModal";

type ServicePlan = {
  id: string;
  name: string;
  speedUp: number;
  speedDown: number;
  price: number;
  mikrotikProfileName: string;
};

export default function ServicePlansPage() {
  const [plans, setPlans] = useState<ServicePlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<ServicePlan | null>(null);

  const fetchPlans = async () => {
    try {
      // By default GetServicePlans only returns active ones (includeInactive=false)
      const res = await api.get("/serviceplans");
      setPlans(res.data);
    } catch (error) {
      console.error("Failed to fetch service plans", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPlans();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this service plan?")) return;
    
    try {
      await api.delete(`/serviceplans/${id}`);
      fetchPlans();
    } catch (error) {
      console.error("Failed to delete service plan", error);
      alert("Failed to delete service plan");
    }
  };

  const handleEdit = (plan: ServicePlan) => {
    setEditingPlan(plan);
    setIsModalOpen(true);
  };

  const handleAdd = () => {
    setEditingPlan(null);
    setIsModalOpen(true);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900">Service Plans</h1>
        <button
          onClick={handleAdd}
          className="flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Add Plan
        </button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Service Plans</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading data...</div>
          ) : plans.length === 0 ? (
            <div className="py-8 text-center text-gray-500">No service plans found.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-gray-500">
                <thead className="bg-gray-50 text-xs uppercase text-gray-700">
                  <tr>
                    <th className="px-6 py-3">Name</th>
                    <th className="px-6 py-3">Speed (Up/Down)</th>
                    <th className="px-6 py-3">Price</th>
                    <th className="px-6 py-3">Mikrotik Profile</th>
                    <th className="px-6 py-3 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {plans.map((plan) => (
                    <tr key={plan.id} className="border-b bg-white hover:bg-gray-50">
                      <td className="px-6 py-4 font-medium text-gray-900">{plan.name}</td>
                      <td className="px-6 py-4">{`${plan.speedUp} Mbps / ${plan.speedDown} Mbps`}</td>
                      <td className="px-6 py-4">Rp {plan.price.toLocaleString("id-ID")}</td>
                      <td className="px-6 py-4">{plan.mikrotikProfileName}</td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() => handleEdit(plan)}
                            className="rounded p-1 text-blue-600 hover:bg-blue-50"
                            title="Edit Plan"
                          >
                            <Edit2 className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => handleDelete(plan.id)}
                            className="rounded p-1 text-red-600 hover:bg-red-50"
                            title="Deactivate Plan"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      <ServicePlanModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={() => {
          setIsModalOpen(false);
          fetchPlans();
        }}
        editingPlan={editingPlan}
      />
    </div>
  );
}
