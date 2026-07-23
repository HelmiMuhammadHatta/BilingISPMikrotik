"use client";

import { useEffect, useState } from "react";
import { format, parseISO } from "date-fns";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CheckCircle2, XCircle } from "lucide-react";

type AuditLog = {
  id: string;
  customerId: string;
  customer: { name: string; pppUsername: string };
  action: number; // 0=Isolir, 1=Restore
  status: string; // "Success" or "Failed"
  executedAt: string;
  errorMessage: string | null;
};

export default function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchLogs() {
      try {
        const res = await api.get("/logs/mikrotikActionLogs");
        setLogs(res.data);
      } catch (error) {
        console.error("Failed to fetch logs", error);
      } finally {
        setLoading(false);
      }
    }
    fetchLogs();
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900">Mikrotik Audit Logs</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recent Action Logs (Max 100)</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading logs...</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-gray-500">
                <thead className="bg-gray-50 text-xs uppercase text-gray-700">
                  <tr>
                    <th className="px-6 py-3">Timestamp</th>
                    <th className="px-6 py-3">Customer</th>
                    <th className="px-6 py-3">Username</th>
                    <th className="px-6 py-3">Action</th>
                    <th className="px-6 py-3">Status</th>
                    <th className="px-6 py-3">Error Message</th>
                  </tr>
                </thead>
                <tbody>
                  {logs.map((log) => (
                    <tr key={log.id} className="border-b bg-white hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        {format(parseISO(log.executedAt), "dd MMM yyyy HH:mm:ss")}
                      </td>
                      <td className="px-6 py-4 font-medium text-gray-900">{log.customer?.name}</td>
                      <td className="px-6 py-4">{log.customer?.pppUsername}</td>
                      <td className="px-6 py-4">
                        <span
                          className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                            log.action === 0
                              ? "bg-red-100 text-red-800"
                              : "bg-blue-100 text-blue-800"
                          }`}
                        >
                          {log.action === 0 ? "Isolir" : "Restore"}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        {log.status === "Success" ? (
                          <div className="flex items-center gap-1 text-green-600">
                            <CheckCircle2 className="h-4 w-4" />
                            <span>Success</span>
                          </div>
                        ) : (
                          <div className="flex items-center gap-1 text-red-600">
                            <XCircle className="h-4 w-4" />
                            <span>Failed</span>
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 text-red-500">
                        {log.errorMessage || "-"}
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
