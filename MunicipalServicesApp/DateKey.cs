using System;

namespace MunicipalServicesApp
{
    /// A tree key made of a date plus the request ID.
    /// Requests are ordered by date first; if two share the exact same
    /// date and time, the ID breaks the tie so neither overwrites the other.

    public struct DateKey : IComparable<DateKey>
    {
        public DateTime Date { get; private set; }
        public int Id { get; private set; }

        public DateKey(DateTime date, int id) : this()
        {
            Date = date;
            Id = id;
        }

        public int CompareTo(DateKey other)
        {
            int byDate = Date.CompareTo(other.Date);
            return byDate != 0 ? byDate : Id.CompareTo(other.Id);
        }
    }
}