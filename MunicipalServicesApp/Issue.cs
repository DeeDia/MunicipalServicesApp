using System;
using System.Collections.Generic;

namespace MunicipalServicesApp
{
    ///<summary>
    // Represents an issue reported by a citizen, including details such as description, location, and status.
    ///</summary>

    public class Issue
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        // Paths to any attached files (images/documents) the user selected
        public List<string> AttachmentPaths { get; set; }

        public DateTime DateReported { get; set; }

        public Issue()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
        }
    }
}
