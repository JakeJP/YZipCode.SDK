using System;
using System.Collections.Generic;
using System.Text;

namespace Yokinsoft.ZipCode.Data.JapanPost
{
    public enum JapanPostDataSource
    {
        UTF,
        Kogaki,
        Oogaki,
        Rome,
        Jigyosyo
    }
    public enum JapanPostHypenType
    {
        FullWidthHyphenMinus, // used in others
        MinusSign // used in UTF version
    }

}
