using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

public interface IParser
{
    string Pattern { get; }

    Regex Regex { get; }

    bool Parse(string line);
}

public abstract class Parser : IParser
{
    public abstract string Pattern { get; }
    public abstract bool Parse(string line);

    private Regex? _regex;

    public Regex Regex => _regex ??= new Regex(Pattern, RegexOptions.Compiled);
}

public abstract class Parser<T> : Parser
{
    public required IProducerConsumerCollection<T> Values { get; set; }

    public override bool Parse(string line)
    {
        var match = Regex.Match(line);
        if (match.Success)
        {
            T? value = CreateValueFromMatch(match);

            if(value is not null)
                Values.TryAdd(value);

            return true;
        }

        return false;
    }

    public abstract T? CreateValueFromMatch(Match match);
}
