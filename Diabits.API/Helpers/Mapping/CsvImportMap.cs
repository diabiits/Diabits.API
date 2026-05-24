using CsvHelper.Configuration;

using Diabits.API.DTOs.HealthDataPoints;

namespace Diabits.API.Helpers.Mapping;

public sealed class CsvImportMap : ClassMap<ImportDto>
{
    public CsvImportMap()
    {
        Map(m => m.DateFrom)
            .Name("Tidsstempel");

        Map(m => m.GlucoseLevel)
            .Name("Indtastning af blodglukose (mmol/l)");

        Map(m => m.CarbGrams)
            .Name("Kulhydratindtag (g)");

        Map(m => m.Units)
            .Name("Injiceret insulin (enh.)");
    }
}