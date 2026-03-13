using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auth.Models
{
  
        public class Department
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? HeadId { get; set; }
            public string HeadName { get; set; } 
        }
    
}