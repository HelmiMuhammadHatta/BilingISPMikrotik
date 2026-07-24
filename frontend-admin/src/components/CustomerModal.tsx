"use client";

import { useState, useEffect } from "react";
import { X } from "lucide-react";
import { api } from "@/lib/api";

type ServicePlan = {
  id: string;
  name: string;
  price: number;
};

export type CustomerForm = {
  id?: string;
  name: string;
  address: string;
  phone: string;
  pppUsername: string;
  pppPassword?: string;
  status: number;
  servicePlanId: string;
};

type Props = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  customerToEdit?: CustomerForm | null;
  servicePlans: ServicePlan[];
};

export function CustomerModal({ isOpen, onClose, onSuccess, customerToEdit, servicePlans }: Props) {
  const [formData, setFormData] = useState<CustomerForm>({
    name: "",
    address: "",
    phone: "",
    pppUsername: "",
    pppPassword: "",
    status: 0,
    servicePlanId: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (customerToEdit) {
      setFormData({
        id: customerToEdit.id,
        name: customerToEdit.name,
        address: customerToEdit.address,
        phone: customerToEdit.phone,
        pppUsername: customerToEdit.pppUsername,
        pppPassword: customerToEdit.pppPassword || "",
        status: customerToEdit.status,
        servicePlanId: customerToEdit.servicePlanId || "",
      });
    } else {
      setFormData({
        name: "",
        address: "",
        phone: "",
        pppUsername: "",
        pppPassword: "",
        status: 0,
        servicePlanId: servicePlans.length > 0 ? servicePlans[0].id : "",
      });
    }
    setError("");
  }, [customerToEdit, servicePlans, isOpen]);

  if (!isOpen) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === "status" ? parseInt(value) : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      if (customerToEdit && customerToEdit.id) {
        await api.put(`/customers/${customerToEdit.id}`, formData);
      } else {
        await api.post("/customers", formData);
      }
      onSuccess();
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.message || "Failed to save customer.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-bold text-gray-900">
            {customerToEdit ? "Edit Customer" : "Add Customer"}
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
            <label className="mb-1 block text-sm font-medium text-gray-700">Name</label>
            <input
              type="text"
              name="name"
              required
              value={formData.name}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Address</label>
            <input
              type="text"
              name="address"
              required
              value={formData.address}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Phone</label>
              <input
                type="text"
                name="phone"
                required
                value={formData.phone}
                onChange={handleChange}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Status</label>
              <select
                name="status"
                value={formData.status}
                onChange={handleChange}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              >
                <option value={0}>Active</option>
                <option value={1}>Isolir</option>
                <option value={2}>Suspended</option>
              </select>
            </div>
          </div>
          
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">PPP Username</label>
              <input
                type="text"
                name="pppUsername"
                required
                value={formData.pppUsername}
                onChange={handleChange}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">PPP Password</label>
              <input
                type="text"
                name="pppPassword"
                required={!customerToEdit}
                value={formData.pppPassword}
                onChange={handleChange}
                placeholder={customerToEdit ? "Leave empty to keep" : ""}
                className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              />
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Service Plan</label>
            <select
              name="servicePlanId"
              required
              value={formData.servicePlanId}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            >
              <option value="" disabled>Select a plan...</option>
              {servicePlans.map((plan) => (
                <option key={plan.id} value={plan.id}>
                  {plan.name} - Rp {plan.price.toLocaleString("id-ID")}
                </option>
              ))}
            </select>
          </div>

          <div className="mt-6 flex justify-end gap-3">
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
              {loading ? "Saving..." : "Save Customer"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
