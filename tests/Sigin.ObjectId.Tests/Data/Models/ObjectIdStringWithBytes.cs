using Newtonsoft.Json;

namespace Sigin.ObjectId.Tests.Data.Models;

public class ObjectIdStringWithBytes
{
    public ObjectIdStringWithBytes(string objectIdString, byte[] bytes)
    {
        String = objectIdString ?? throw new ArgumentNullException(nameof(objectIdString));
        Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
    }

    public string String { get; }

    public byte[] Bytes { get; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
