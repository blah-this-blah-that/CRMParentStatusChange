using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParentStatusChangeGenericPlugin
{
    public class ChildEntity
    {
        public string ChildEntityName { get; set; }
        public string LookupSchemaName { get; set; }
        public string PrimaryColumnName { get; set; }
        public int ChildActiveStatecode { get; set; }
        public int ChildInactiveStatecode { get; set; }
        public int ChildActiveStatuscode { get; set; }
        public int ChildInactiveStatuscode { get; set; }
    }
}
