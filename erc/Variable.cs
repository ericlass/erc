using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class Variable
    {
        public string Name { get; set; } 

        public DataType DataType { get; set; }

        //Used for arrays etc.
        public DataType SubDataType { get; set; }
    }
}
