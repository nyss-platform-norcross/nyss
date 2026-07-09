const sortOrder = ['Pending', 'New', 'Accepted', 'Rejected', 'Closed'];

export const sortByReportStatus = (a, b) => {
  const statusA = a.status === 'New' ? 'Pending' : a.status;
  const statusB = b.status === 'New' ? 'Pending' : b.status;

  const timeA = new Date(a.dateTime || a.receivedAt).getTime();
  const timeB = new Date(b.dateTime || b.receivedAt).getTime();

  // If statuses are different, compare based on sortOrder
  if (statusA !== statusB) {
    return sortOrder.indexOf(statusA) - sortOrder.indexOf(statusB);
  }

  // If statuses are the same, compare based on time
  return timeB - timeA;
}