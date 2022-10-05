using Microsoft.Extensions.Logging;
using Orleans.Streams.Core;
using Orleans.Streams;
using Orleans;
using nsPocOrlns.Grains.Interfaces;
using nsPocOrlns.Common;
using Orleans.Runtime;

namespace nsPocOrlns.Grains;

[ImplicitStreamSubscription("ehns-nspoc-orleans")]
public class ConsumerGrain : Grain, IConsumerGrain, IStreamSubscriptionObserver
{
    private readonly ILogger<IConsumerGrain> _logger;
    private readonly LoggerObserver _observer;
    private readonly IPersistentState<ConsumerState> _consumerState;
    private bool _isDirty = false;

    public long LastSequence
    {
        get => _consumerState.State.LastSeq;
        set
        {
            _consumerState.State.LastSeq = value;
            _isDirty = true;
        }
    }

    public ConsumerGrain(ILogger<IConsumerGrain> logger, [PersistentState("consumerState")]  IPersistentState<ConsumerState> consumerState)
    {
        _logger = logger;
        _observer = new LoggerObserver(_logger, this);
        _consumerState = consumerState;
    }

    Task TimerCallback(object callbackstate)
    {
        if (_isDirty)
        {
            _consumerState.WriteStateAsync();
            _isDirty = false;
        }
        return Task.CompletedTask;
    }


    // Called when a subscription is added
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
    {
        // Plug our LoggerObserver to the stream
        var handle = handleFactory.Create<UnitEvent>();
        await handle.ResumeAsync(_observer);
    }

    public override Task OnActivateAsync()
    {
        _logger.LogInformation("OnActivateAsync");
        this.RegisterTimer(this.TimerCallback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }


    /// <summary>
    /// Class that will log streaming events
    /// </summary>
    private class LoggerObserver : IAsyncObserver<UnitEvent>
    {
        private readonly ILogger<IConsumerGrain> _logger;
        private readonly ConsumerGrain _consumerGrain;

        public LoggerObserver(ILogger<IConsumerGrain> logger, ConsumerGrain grain)
        {
            _logger = logger;
            _consumerGrain = grain;
        }


        public Task OnCompletedAsync()
        {
            _logger.LogInformation("OnCompletedAsync");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogInformation("OnErrorAsync: {Exception}", ex);
            return Task.CompletedTask;
        }

        public Task OnNextAsync(UnitEvent item, StreamSequenceToken? token = null)
        {
            if (token != null)
            {
                if ( token.SequenceNumber <= _consumerGrain.LastSequence )
                {
                    return Task.CompletedTask;
                }
                else
                {
                    _consumerGrain.LastSequence = token.SequenceNumber;
                }
                
            }
            var unitGrain = _consumerGrain.GrainFactory.GetGrain<IUnitGrain>(item.Imei);
            unitGrain.ProcessEvent(item);
            _logger.LogInformation("OnNextAsync: Stream: {Stream} item: {Item}, token = {Token}", _consumerGrain.GetPrimaryKey(), item, token);
            return Task.CompletedTask;
        }
    }

}
