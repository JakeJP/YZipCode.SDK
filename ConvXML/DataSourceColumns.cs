using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yokinsoft.ZipCode.ConvXML
{
    public class DataSourceColumns
    {
        public DataSourceColumns()
        {
            foreach (var item in GetType().GetFields())
            {
                item.SetValue(this, -1);
            }
        }
        public int JISCode;
        public int PostalCode5;
        public int PostalCode;
        public int PrefectureRuby;
        public int CityRuby;
        public int TownRuby;
        public int Prefecture;
        public int City;
        public int Town;
        public int TownHasMultipleCodes;
        public int KoazaHasBlockNumbers;
        public int TownHasBlockNumbers;
        public int TownsSharePostalCode;
        public int UpdateStatus;
        public int UpdateFor;

        public int Koaza;
        public int BusinessRuby;
        public int Business;
        public int PostOffice;
        public int POB;
        public int POBIndex;
    }

}
