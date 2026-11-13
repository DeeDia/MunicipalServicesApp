using System;
using System.Collections.Generic;

namespace MunicipalServicesApp
{
    /// The stages every service request moves through, in order.
    /// An enum stops us mistyping a status as free text, and the numeric
    /// order (0 to 3) lets us calculate progress.
    public enum RequestStatus
    {
        Submitted = 0,
        UnderReview = 1,
        InProgress = 2,
        Resolved = 3
    }

    /// One entry in a request's history: what it became, and when.
    public class StatusChange
    {
        public RequestStatus Status { get; set; }
        public DateTime When { get; set; }
    }

    /// A citizen's service request, tracked from submission to resolution.
    public class ServiceRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }

        // The area of the municipality (a node in our location graph)
        public string Area { get; set; }

        // The free-text location the citizen typed
        public string Location { get; set; }
        public string Description { get; set; }

        // 1 = most urgent, 3 = routine
        public int Priority { get; set; }

        public DateTime DateSubmitted { get; set; }

        // private set: the status can only change through SetStatus/Advance,
        // so the history list can never disagree with the current status
        public RequestStatus Status { get; private set; }

        public List<StatusChange> History { get; private set; }
        public List<string> AttachmentPaths { get; private set; }

        public ServiceRequest()
        {
            History = new List<StatusChange>();
            AttachmentPaths = new List<string>();
            Status = RequestStatus.Submitted;
        }
        /// The unique identifier the citizen uses to track the request, e.g. SR-1001.
        public string Reference
        {
            get { return "SR-" + Id; }
        }

        public bool IsOpen
        {
            get { return Status != RequestStatus.Resolved; }
        }

        /// Human-readable status ("Under Review" rather than "UnderReview").
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case RequestStatus.UnderReview: return "Under Review";
                    case RequestStatus.InProgress: return "In Progress";
                    default: return Status.ToString();
                }
            }
        }
        /// 0, 33, 66 or 100: used to drive the progress bar.
        public int ProgressPercent
        {
            get { return ((int)Status * 100) / 3; }
        }
        /// Records a status and when it happened.
        
        public void SetStatus(RequestStatus status, DateTime when)
        {
            Status = status;
            History.Add(new StatusChange { Status = status, When = when });
        }

        /// Moves the request to its next stage. Returns false if it is already resolved.
        
        public bool Advance()
        {
            if (Status == RequestStatus.Resolved)
                return false;

            SetStatus((RequestStatus)((int)Status + 1), DateTime.Now);
            return true;
        }

        // What the user sees in list boxes
        public override string ToString()
        {
            return Reference + "  |  " + StatusText + "  |  " + Category + "  |  " + Area;
        }
    }
    /// Maps each request category to the municipal department that handles it.
    /// This is what gives the general tree its Department -> Category levels.
    public static class DepartmentMap
    {
        public static string DepartmentFor(string category)
        {
            switch (category)
            {
                case "Roads":
                case "Utilities (Water/Electricity)":
                    return "Infrastructure";

                case "Sanitation":
                case "Waste Management":
                    return "Environmental Services";

                case "Public Safety":
                    return "Safety and Security";

                default:
                    return "General Services";
            }
        }
    }
}