import dayjs from "dayjs";
import utc from "dayjs/plugin/utc";
import weekOfYear from "dayjs/plugin/weekOfYear";
dayjs.extend(utc);
dayjs.extend(weekOfYear);

export const formatDate = (date) => {
  const year = date.getFullYear();
  const month = date.getMonth() + 1;
  const day = date.getDate();
  const humanReadableMonth = `${month < 10 ? "0" + month : month}`;
  const humanReadableDay = `${day < 10 ? "0" + day : day}`;

  return `${year}-${humanReadableMonth}-${humanReadableDay}`;
};

export const convertToLocalDate = (date) => {
  const utcOffset = Math.floor(dayjs().utcOffset() / 60);
  const utcDate = dayjs(date);
  return utcDate.add(utcOffset, "hour");
};

export const convertToUtc = (date) => {
  const utcOffset = Math.floor(dayjs().utcOffset() / 60);
  const localeDate = dayjs(date);
  return localeDate.add(-utcOffset, "hour");
};

export const getUtcOffset = () => Math.floor(dayjs().utcOffset() / 60);

export const getEpiWeek = () => {
  const date = dayjs();

  const dayPs = (date.date() + 7) % 7;
  date.set("date", date.date() - dayPs + 3);

  const jan4 = dayjs(new Date(date.year(), 0, 4));

  const dayDifference = (date - jan4) / 86400000;

  if (dayjs(date.year(), 0, 1).day() < 4) {
    return 1 + Math.ceil(dayDifference / 7);
  } else {
    return Math.ceil(dayDifference / 7);
  }
};

export const generateEpiWeekFilters = () => {
  const currentEpiWeek = getEpiWeek() - 1;
  const weekFilters = [];

  for (let i = currentEpiWeek - 7; i <= currentEpiWeek; i++) {
    weekFilters.push({
      epiWeek: i,
      reportingCorrectly: true,
      reportingWithErrors: true,
      notReporting: true,
    });
  }

  return weekFilters;
};
