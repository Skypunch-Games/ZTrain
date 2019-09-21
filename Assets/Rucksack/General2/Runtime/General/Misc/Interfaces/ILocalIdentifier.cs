using System;

namespace Devdog.General2
{
    public interface ILocalIdentifier : IEquatable<ILocalIdentifier>
    {
        string ID { get; }

        string ToString();
    }
}
