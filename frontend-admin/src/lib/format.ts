export function formatInvoicePeriod(start: Date | null, end: Date | null): string | null {
  if (!start || !end) return null;
  
  // Check if dates are valid
  if (isNaN(start.getTime()) || isNaN(end.getTime())) {
    return null;
  }

  const formatter = new Intl.DateTimeFormat('id-ID', { day: 'numeric', month: 'short', year: 'numeric' });
  
  const startYear = start.getFullYear();
  const startMonth = start.getMonth();
  
  const endYear = end.getFullYear();
  const endMonth = end.getMonth();

  if (startYear === endYear && startMonth === endMonth) {
    // Same month and year: "1 – 31 Jan 2025"
    return `${start.getDate()} – ${formatter.format(end)}`;
  } else if (startYear === endYear) {
    // Same year, different month: "25 Jan – 24 Feb 2025"
    const startStr = new Intl.DateTimeFormat('id-ID', { day: 'numeric', month: 'short' }).format(start);
    return `${startStr} – ${formatter.format(end)}`;
  } else {
    // Different year: "25 Des 2024 – 24 Jan 2025"
    return `${formatter.format(start)} – ${formatter.format(end)}`;
  }
}
