namespace FinabitEmployee.Data
{
     public class Asset
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SerialNumber { get; set; }
            public DateTime PurchaseDate { get; set; }
            public DateTime WarrantyExpiry { get; set; }
            public string Status { get; set; } // Available, Assigned, Returned
        }

        public class AssetAssignment
        {
            public int Id { get; set; }
            public int AssetId { get; set; }
            public int EmployeeId { get; set; }
            public DateTime AssignedDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public string Condition { get; set; } // New, Good, Used
        }

        public class AssetReturn
        {
            public int Id { get; set; }
            public int AssetId { get; set; }
            public int EmployeeId { get; set; }
            public DateTime ReturnedDate { get; set; }
            public string Condition { get; set; } // Good, Damaged, Lost
        }
        public class AssetRequest
        {
            public int Id { get; set; }
            public int AssetId { get; set; }
            public int EmployeeId { get; set; }
            public DateTime RequestDate { get; set; }
            public RequestStatus Status { get; set; } = RequestStatus.Pending;

            // Navigation properties
            public Asset Asset { get; set; }
         
        }
        public enum RequestStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }
    }

