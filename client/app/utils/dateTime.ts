export function convertTo12HourTime(date: Date) {
  const hours = date.getHours();
  const minutes = date.getMinutes();

  return {
    hours: hours % 12 || 12, // Converts 0 to 12, keeps 1-11 as is, converts 13-23 to 1-11
    minutes: minutes < 10 ? `0${minutes}` : String(minutes),
    meridiem: hours < 12 ? "AM" : "PM",
  };
}
