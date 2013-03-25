using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParentStatusChangeGenericPlugin
{
    public class ParentEntity
    {
        public string ParentEntityName { get; set; }
        public int ParentActiveStatecode { get; set; }
        public int ParentInactiveStatecode { get; set; }
    }
}
