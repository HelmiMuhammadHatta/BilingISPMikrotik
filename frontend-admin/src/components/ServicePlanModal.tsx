"use client";

import { useState, useEffect } from "react";
import { X } from "lucide-react";
import { api } from "@/lib/api";

type ServicePlan = {
  id?: string;
  name: string;
  speedUp: number;
  speedDown: number;
  price: number;
  mikrotikProfileName: string;
};

type Props = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  editingPlan: ServicePlan | null;
};

export function ServicePlanModal({ isOpen, onClose, onSuccess, editingPlan }: Props) {
  const [formData, setFormData] = useState<ServicePlan>({
    name: "",
    speedUp: 0,
    speedDown: 0,
    price: 0,
    mikrotikProfileName: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (editingPlan) {
      setFormData({
        name: editingPlan.name,
        speedUp: editingPlan.speedUp,
        speedDown: editingPlan.speedDown,
        price: editingPlan.price,
        mikrotikProfileName: editingPlan.mikrotikProfileName,
      });
    } else {
      setFormData({
        name: "",
        speedUp: 0,
        speedDown: 0,
        price: 0,
        mikrotikProfileName: "",
      });
    }
  }, [editingPlan, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      if (editingPlan?.id) {
        await api.put(`/serviceplans/${editingPlan.id}`, formData);
      } else {
        await api.post("/serviceplans", formData);
      }
      onSuccess();
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.message || "Failed to save service plan.");
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "number" ? Number(value) : value,
    }));
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-bold text-gray-900">
            {editingPlan ? "Edit Service Plan" : "Add Service Plan"}
          </h2>
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Plan Name</label>
            <input
              type="text"
              name="name"
              required
              value={formData.name}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              placeholder="e.g. Basic 10Mbps"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Speed Up (Mbps)</label>
              <input
                type="number"
                name="speedUp"
                required
                min={1}
                value={formData.speedUp}
                onChange={handleChange}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Speed Down (Mbps)</label>
              <input
                type="number"
                name="speedDown"
                required
                min={1}
                value={formData.speedDown}
                onChange={handleChange}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              />
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Price (Rp)</label>
            <input
              type="number"
              name="price"
              required
              min={0}
              value={formData.price}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Mikrotik Profile Name</label>
            <input
              type="text"
              name="mikrotikProfileName"
              required
              value={formData.mikrotikProfileName}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              placeholder="e.g. profile-10m"
            />
          </div>

          <div className="mt-6 flex justify-end gap-3 pt-4 border-t">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="rounded-md px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {loading ? "Saving..." : "Save Plan"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
