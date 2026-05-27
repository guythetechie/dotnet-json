using CsCheck;
using System;
using System.Linq;

namespace common;

public static class HttpGenerator
{
    public static Gen<Uri> AbsoluteUri { get; } =
        from scheme in Gen.OneOfConst("http", "https")
        from tld in Gen.OneOfConst("com", "net", "org", "io")
        from host in
            from host in Gen.String.AlphaNumeric
            where string.IsNullOrWhiteSpace(host) is false
            select $"{host.ToLowerInvariant()}.{tld}"
        from paths in
            from paths in Gen.String.AlphaNumeric.Array[0, 5]
            let normalizedPaths = paths.Select(path => path.ToLowerInvariant())
            select string.Join('/', normalizedPaths)
        let builder = new UriBuilder
        {
            Scheme = scheme,
            Host = host,
            Path = string.Join('/', paths)
        }
        select builder.Uri;
}