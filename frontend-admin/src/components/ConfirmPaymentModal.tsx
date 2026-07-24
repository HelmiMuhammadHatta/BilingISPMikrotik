"use client";

import { useState } from "react";
import { X, CheckCircle } from "lucide-react";
import { api } from "@/lib/api";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  invoiceId: string | null;
  amount: number;
};

export function ConfirmPaymentModal({ isOpen, onClose, onSuccess, invoiceId, amount }: Props) {
  const [method, setMethod] = useState("Transfer Bank");
  const [referenceNumber, setReferenceNumber] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  if (!isOpen || !invoiceId) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      await api.post("/payments/confirm", {
        invoiceId,
        method,
        amount,
        referenceNumber,
      });
      setSuccess(true);
      setTimeout(() => {
        setSuccess(false);
        setReferenceNumber("");
        onSuccess();
      }, 1500);
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.error || err.response?.data?.message || "Failed to confirm payment.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-sm rounded-lg bg-white p-6 shadow-xl">
        {success ? (
          <div className="flex flex-col items-center justify-center py-6 text-center">
            <CheckCircle className="h-16 w-16 text-green-500 mb-4" />
            <h2 className="text-xl font-bold text-gray-900">Payment Confirmed!</h2>
            <p className="text-sm text-gray-500 mt-2">The invoice has been marked as paid.</p>
          </div>
        ) : (
          <>
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-lg font-bold text-gray-900">Confirm Payment</h2>
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
                <label className="mb-1 block text-sm font-medium text-gray-700">Payment Method</label>
                <select
                  required
                  value={method}
                  onChange={(e) => setMethod(e.target.value)}
                  className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                >
                  <option value="Transfer Bank">Transfer Bank</option>
                  <option value="Cash">Cash</option>
                  <option value="E-Wallet">E-Wallet</option>
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Amount Received</label>
                <input
                  type="text"
                  readOnly
                  value={`Rp ${amount.toLocaleString("id-ID")}`}
                  className="w-full rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-gray-500 outline-none"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Reference Number</label>
                <input
                  type="text"
                  required
                  value={referenceNumber}
                  onChange={(e) => setReferenceNumber(e.target.value)}
                  placeholder="e.g. TRF-12345"
                  className="w-full rounded-md border border-gray-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                />
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
                  className="rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
                >
                  {loading ? "Confirming..." : "Confirm Payment"}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
}
