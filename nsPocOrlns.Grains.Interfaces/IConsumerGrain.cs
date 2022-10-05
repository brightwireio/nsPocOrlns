using Orleans;
using System.Runtime.CompilerServices;

namespace nsPocOrlns.Grains.Interfaces;

public interface IConsumerGrain : IGrainWithGuidKey
{
}


public class ConsumerState
{
    public long LastSeq { get; set; }

}