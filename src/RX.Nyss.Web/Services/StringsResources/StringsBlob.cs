using System.Collections.Generic;

namespace RX.Nyss.Web.Services.StringsResources
{
    public class StringsBlob
    {
        public IEnumerable<Entry> Strings { get; set; }

        public class Entry
        {
            public string Key { get; set; }

            public IDictionary<string, string> Translations { get; set; }

            public string GetTranslation(string languageCode) =>
                (Translations.ContainsKey(languageCode) ? Translations[languageCode] : default) ?? Key;
        }
    }
}
