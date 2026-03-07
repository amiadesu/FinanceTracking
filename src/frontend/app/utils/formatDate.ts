export function formatDate(dateString?: string, includeTime: boolean = false) {
  if (!dateString) return '';
  if (includeTime) {
    return new Date(dateString).toLocaleString();
  }
  return dateString.split('T')[0]; 
}