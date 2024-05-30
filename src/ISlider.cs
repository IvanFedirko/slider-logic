using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using System.Reactive.Linq;


namespace SliderLogic
{
    public interface ISlider
    {
        event EventHandler<string> Update;
        event EventHandler<int> EventValueChange;
        
        Task ChangeValue(bool inc);
        Task Reset(int value);
        Task Activate(CancellationToken token);
    }
}