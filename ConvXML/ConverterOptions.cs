using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yokinsoft.ZipCode.ConvXML;
using Yokinsoft.ZipCode.Data.JapanPost;
using Yokinsoft.ZipCode.Data;

namespace Yokinsoft.ZipCode.ConvXML
{
    internal class ConverterOptions
    {
        public JapanPostDataSource OriginalDataSource { get; set; }
        public KanaType OutputKanaType { get; set; }
        public bool IncludeJigyosyoData { get; set; } 
        public bool XMLIndent { get; set; }

    }
}
