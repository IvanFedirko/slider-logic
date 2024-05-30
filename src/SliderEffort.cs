using System.Reactive.Linq;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FIV.SliderLogic
{


    public class SliderEffort : ISlider
    {
        public event EventHandler<string> Update;
        Stopwatch stopWatch = new Stopwatch();
        public event EventHandler<int> EventValueChange;



        protected int _minValue { get; set; }
        protected int _maxValue { get; set; }


        Func<string> _labelMinValue { get; set; }
        Func<string> _labelMaxValue { get; set; }
        Func<string> _labelCurrentValue { get; set; }
        protected int _dafaultValue { get; set; }
        protected readonly int _initValue = 0;


        private int _timeout = 0;
        public SliderEffort(int minValue, int maxValue, int initValue, int timeout)
        {
            _timeout = timeout;

            _minValue = minValue;
            _maxValue = maxValue;
            _initValue = initValue;
            _dafaultValue = initValue;
        }


        public async Task Reset(int value)
        {
            _dafaultValue = value;

            await Task.Run(async () =>
            {
                EventValueChange?.Invoke(null, _dafaultValue);
                var val = await StringConstructor();
                Update?.Invoke(null, val);
            });
        }



        protected async Task<string> StringConstructor()
        {

            return await Task.Run(() => $"[{_minValue} :{_dafaultValue}: {_maxValue}]");
        }




        public async Task ChangeValue(bool inc)
        {
             stopWatch.Restart();
            await Task.Run(async () =>
                        {
                            if (inc) { _dafaultValue = _minValue; }
                            else if (!inc) { _dafaultValue = _maxValue; }
                            else return;
                           
                           // _eventValueChange?.Invoke(null, _dafaultValue);
                            EventValueChange?.Invoke(null, _dafaultValue);
                            var val = await StringConstructor();
                            Update?.Invoke(null, val);
                        }
                        );

        }

        public async Task Activate(CancellationToken token)
        {
            Task observ = Task.Run(async () =>
            {


                var val = await StringConstructor();
                Update?.Invoke(null, val);

                Observable.Interval(TimeSpan.FromMilliseconds(1))
                    .Timestamp()
                    .Select(x => x)
                    .Subscribe(async (x) =>
                    {
                        if (stopWatch.IsRunning && stopWatch.ElapsedMilliseconds > 40)
                        {
                            stopWatch.Stop();
                            _dafaultValue = _initValue;
                            EventValueChange?.Invoke(null, _dafaultValue);

                            var val = await StringConstructor();
                            Update?.Invoke(null, val);
                        }
                    });

            });

            await Task.WhenAll(
             observ,
             Task.Delay(Timeout.Infinite, token)
         );


        }



    }
}