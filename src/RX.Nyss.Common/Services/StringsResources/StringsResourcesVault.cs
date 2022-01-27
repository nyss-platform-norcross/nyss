using System;
using System.Collections.Generic;
using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.Common.Services.StringsResources;

public class StringsResourcesVault
{
    private readonly IDictionary<string, StringResourceValue> _resources;

    public StringsResourcesVault(IDictionary<string, StringResourceValue> resources)
    {
        _resources = resources;
    }

    /// <summary>
    /// Returns translation for a key
    /// </summary>
    /// <param name="key">Translation key</param>
    /// <returns>Translated key</returns>
    /// <exception cref="Exception">Throws exception if key is not found.</exception>
    public string Get(string key)
    {
        if (!_resources.TryGetValue(key, out var stringResourceValue))
        {
            throw new Exception($"Could not find translations for {key}");
        }

        return stringResourceValue.Value;
    }

    /// <summary>
    /// Returns translation for a key. If translation by key will not be found a key is return.
    /// </summary>
    /// <param name="key">Translation key</param>
    public string this[string key] =>
        _resources.TryGetValue(key, out var stringResourceValue) ? stringResourceValue.Value : key;
}