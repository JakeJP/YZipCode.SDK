using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using Yokinsoft.ZipCode.Data;

namespace Yokinsoft.ZipCode.Web
{
    public static class PostCodeLookupResponseExtentions
    {
        public static LookupObjectResponse AsObject( this LookupXmlResponse response )
        {
            return new LookupObjectResponse(response);
        }
    }
}
