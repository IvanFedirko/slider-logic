using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SliderLogic
{


    public class Slider : ISlider
    {

        public event EventHandler<string> Update;

        public event EventHandler<int> EventValueChange;

        protected event EventHandler<int> _eventValueChange;

        protected int _minValue { get; set; }
        protected int _maxValue { get; set; }

        int iteration = 1;

        Func<string> _labelMinValue { get; set; }
        Func<string> _labelMaxValue { get; set; }
        Func<string> _labelCurrentValue { get; set; }
        int _currentValue { get; set; }


        public Slider(int minValue, int maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _currentValue = (_maxValue - Math.Abs(_minValue)) / 2;
        }

        public Slider(int minValue, int maxValue, int currentState)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _currentValue = currentState;
        }

        public async Task Reset(int value)
        {
            _currentValue = value;

            await Task.Run(async () =>
            {
                EventValueChange?.Invoke(null, _currentValue);
                var val = await StringConstructor();
                Update?.Invoke(null, val);
            });
        }

        public async Task Activate(CancellationToken token)
        {
          Task observ = Task.Run(async () =>
          {
              Observable.FromEventPattern<EventHandler<int>, int>(
                     handler => (s, a) => handler(s, a),
                     handler => _eventValueChange += handler,
                     handler => _eventValueChange -= handler)
                     .Select(x => x)
                     .Throttle(TimeSpan.FromMilliseconds(80))
                     .Subscribe(x =>
                       { iteration = 1; 
                        EventValueChange?.Invoke(null, _currentValue); 
                       }
                 );

              var val = await StringConstructor();
              Update?.Invoke(null, val);

         });

               await Task.WhenAll(
                observ,
                Task.Delay(Timeout.Infinite, token)
            );

        }

        private async Task KeyReader()
        {
            await Task.Run(async () =>
            {
                var res = await StringConstructor();
            });
        }

        protected  async Task<string> StringConstructor()
        {

            return await Task.Run(() => $"[{_minValue} :{_currentValue}: {_maxValue}]");
        }

        bool CheckValueMax()
        {
            var tmp = iteration;

            return ((_currentValue + tmp) <= _maxValue);
        }

        bool CheckValueMin()
        {
            var tmp = iteration;

            return ((_currentValue - tmp) >= _minValue);
        }

        public async Task ChangeValue(bool inc)
        {
            await Task.Run(async () =>
            {
                if (inc && CheckValueMax()) { _currentValue = _currentValue + iteration; iteration++; }
                else if (!inc && CheckValueMin()) { _currentValue = _currentValue - iteration; iteration++; }
                else return;

                _eventValueChange?.Invoke(null, _currentValue);
                var val = await StringConstructor();
                Update?.Invoke(null, val);
            }

            );

        }

    }


}

