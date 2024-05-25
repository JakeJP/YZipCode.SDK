using System;
using System.Collections.Generic;
using System.Text;

namespace Yokinsoft.ZipCode.Data.JapanPost
{
    public enum DataSourceColumn
    {
        JISCode,
        PostalCode5,
        PostalCode,
        PrefectureRuby,
        CityRuby,
        TownRuby,
        Prefecture,
        City,
        Town,
        TownHasMultipleCodes,
        KoazaHasBlockNumbers,
        TownHasBlockNumbers,
        TownsSharePostalCode,
        UpdateStatus,
        UpdateFor,

        Koaza,
        BusinessRuby,
        Business,
        PostOffice,
        POB,
        POBIndex,
    }
}
