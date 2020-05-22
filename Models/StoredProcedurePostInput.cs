using System.Collections.Generic;

namespace StoredProceduresWebApi.Models
{
    public class StoredProcedurePostInput
    {
        public string SPName { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public int? Timeout { get; set; }
    }
}
