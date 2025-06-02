export function convertTo12HourTime(date: Date) {
  const dateHours = date.getHours();
  const convertedHours = dateHours <= 12 ? dateHours : dateHours - 12;
  const meridiem = dateHours <= 12 ? "AM" : "PM";

  return {
    hours: convertedHours,
    minutes: date.getMinutes(),
    meridiem,
  };
}
