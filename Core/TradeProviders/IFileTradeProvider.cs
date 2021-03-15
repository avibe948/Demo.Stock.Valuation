using Domain.Trade;
using System;
using System.Threading;

namespace Cibc.Core.TradeProviders
{
    public interface IFileTradeProvider<out TTrade> where TTrade:Trade
    {
        // loading sync will be faster, but the idea  to use AsyncEnumerable to show that I understand the concept. In real scenarios (Network rather than local machine disk) using async may be more efficient if the network is slow.   
        // This is a pull model and in real applications normally a push model is much better, 
        IObservable<TTrade> GetTradeStream(CancellationToken cancellationToken = default);

        FileFormat FileFormat { get; }
               
        string FilePath { get; }

    }

}
