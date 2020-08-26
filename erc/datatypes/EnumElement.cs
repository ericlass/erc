using System;

namespace erc
{
    public class EnumElement
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public EnumElement(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }
}
