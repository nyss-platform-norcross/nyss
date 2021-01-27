import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
dayjs.extend(utc);

export const formatDate = (date) => {
  const year = date.getFullYear();
  const month = date.getMonth() + 1;
  const day = date.getDate();
  const humanReadableMonth = `${month < 10 ? '0' + month : month}`;
  const humanReadableDay = `${day < 10 ? '0' + day : day}`;

  return `${year}-${humanReadableMonth}-${humanReadableDay}`;
}

export const convertToLocalDate = (date) => {
  const utcOffset = Math.floor(dayjs().utcOffset() / 60);
  const utcDate = dayjs(date);
  return utcDate.add(utcOffset, 'hour');
}

export const convertToUtc = (date) => {
  const utcOffset = Math.floor(dayjs().utcOffset() / 60);
  const localeDate = dayjs(date);
  return localeDate.add(-utcOffset, 'hour');
}

export const getUtcOffset = () => 
  Math.floor(dayjs().utcOffset() / 60);