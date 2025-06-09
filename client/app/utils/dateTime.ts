export function convertTo12HourTime(date: Date) {
  const hours = date.getHours();
  const minutes = date.getMinutes();

  return {
    hours: hours % 12 || 12, // Converts 0 to 12, keeps 1-11 as is, converts 13-23 to 1-11
    minutes: minutes < 10 ? `0${minutes}` : String(minutes),
    meridiem: hours < 12 ? "AM" : "PM",
  };
}

export function formatDateHeader(date: Date): string {
  const now = new Date();
  const currentYear = now.getFullYear();
  const messageYear = date.getFullYear();

  const monthNames = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
  ];

  const monthName = monthNames[date.getMonth()];
  const day = date.getDate();

  if (messageYear === currentYear) {
    return `${monthName} ${day}`;
  } else {
    return `${monthName} ${day}, ${messageYear}`;
  }
}

export function isSameDay(date1: Date, date2: Date): boolean {
  return (
    date1.getFullYear() === date2.getFullYear() &&
    date1.getMonth() === date2.getMonth() &&
    date1.getDate() === date2.getDate()
  );
}

export function isToday(date: Date): boolean {
  const today = new Date();
  return isSameDay(date, today);
}
