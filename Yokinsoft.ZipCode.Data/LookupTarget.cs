
using System;
using System.ComponentModel;

namespace Yokinsoft.ZipCode.Data
{
    [Flags]
    public enum LookupTarget
    {
        ZipCode = 1,
        Address = 2,
        Place = 4,
        All = 7,
        //Abbreviations
        a = Address,
        z = ZipCode,
        p = Place
    }
}
