export const formatDate = (date) => {
  const year = date.getFullYear();
  const month = date.getMonth() + 1;
  const day = date.getDate();
  const humanReadableMonth = `${month < 10 ? '0' + month : month}`;
  const humanReadableDay = `${day < 10 ? '0' + day : day}`;

  return `${year}-${humanReadableMonth}-${humanReadableDay}`;
}