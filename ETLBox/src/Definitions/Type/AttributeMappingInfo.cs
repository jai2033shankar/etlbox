﻿using System.Reflection;

namespace ETLBox.DataFlow
{
    internal class AttributeMappingInfo
    {
        internal PropertyInfo PropInInput { get; set; }
        internal string PropNameInInput { get; set; }
        internal PropertyInfo PropInOutput { get; set; }
        internal string PropNameInOutput { get; set; }
    }
}
